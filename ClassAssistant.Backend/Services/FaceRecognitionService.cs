using System.Collections.Concurrent;
using OpenCvSharp;
using ClassAssistant.Backend.Data;
using ClassAssistant.Backend.Models;

namespace ClassAssistant.Backend.Services;

public interface IFaceRecognitionService
{
    (byte[] ProcessedFrame, List<string> RecognizedNames) ProcessFrame(
        byte[] frameBytes,
        string courseCode,
        string sessionId,
        string selectedDateStr,
        IServiceScopeFactory scopeFactory);

    (byte[] ProcessedFrame, int CapturedCount, bool IsComplete) ProcessRegistrationFrame(
        int studentId,
        byte[] frameBytes,
        string datasetPath);
}

public class FaceRecognitionService : IFaceRecognitionService
{
    private readonly CascadeClassifier _faceClassifier;
    private readonly ConcurrentDictionary<string, double> _lastDbUpdateTimes = new();
    private readonly ConcurrentDictionary<int, int> _registrationCounts = new();

    public FaceRecognitionService()
    {
        string cascadePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "haarcascade_frontalface_default.xml");
        if (File.Exists(cascadePath))
        {
            _faceClassifier = new CascadeClassifier(cascadePath);
        }
        else
        {
            _faceClassifier = new CascadeClassifier();
        }
    }

    public (byte[] ProcessedFrame, List<string> RecognizedNames) ProcessFrame(
        byte[] frameBytes,
        string courseCode,
        string sessionId,
        string selectedDateStr,
        IServiceScopeFactory scopeFactory)
    {
        var recognizedNames = new List<string>();
        if (frameBytes == null || frameBytes.Length == 0)
            return (Array.Empty<byte>(), recognizedNames);

        try
        {
            using var srcMat = Cv2.ImDecode(frameBytes, ImreadModes.Color);
            if (srcMat.Empty())
                return (Array.Empty<byte>(), recognizedNames);

            using var smallMat = new Mat();
            Cv2.Resize(srcMat, smallMat, new Size(0, 0), 0.5, 0.5, InterpolationFlags.Linear);

            using var grayMat = new Mat();
            Cv2.CvtColor(smallMat, grayMat, ColorConversionCodes.BGR2GRAY);

            Rect[] faces = Array.Empty<Rect>();
            if (!_faceClassifier.Empty())
            {
                faces = _faceClassifier.DetectMultiScale(grayMat, scaleFactor: 1.1, minNeighbors: 4, minSize: new Size(20, 20));
            }

            DateTime nowDt = DateTime.UtcNow;
            DateTime targetDate = DateTime.UtcNow.Date;
            if (!string.IsNullOrWhiteSpace(selectedDateStr) && DateTime.TryParse(selectedDateStr, out var parsedDate))
            {
                targetDate = parsedDate.Date;
            }

            if (faces.Length > 0)
            {
                using var scope = scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var students = dbContext.Students.ToList();

                foreach (var face in faces)
                {
                    var matchedStudent = students.FirstOrDefault();
                    string studentName = matchedStudent != null ? matchedStudent.Name : "Student";

                    recognizedNames.Add(studentName);

                    if (matchedStudent != null)
                    {
                        string cacheKey = $"{matchedStudent.Id}_{courseCode}_{targetDate:yyyy-MM-dd}_{sessionId}";
                        double nowTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        double lastUpdate = _lastDbUpdateTimes.TryGetValue(cacheKey, out var val) ? val : 0;

                        if (nowTs - lastUpdate > 10)
                        {
                            _lastDbUpdateTimes[cacheKey] = nowTs;

                            var att = dbContext.Attendances.FirstOrDefault(a =>
                                a.StudentId == matchedStudent.Id &&
                                a.Date.Date == targetDate &&
                                (string.IsNullOrEmpty(courseCode) || a.CourseCode == courseCode));

                            if (att != null)
                            {
                                att.LastSeenTime = nowDt;
                                att.TotalMinutes = (int)(att.LastSeenTime - att.EntryTime).TotalMinutes;
                                att.IsPresent = 1.0;
                                if (!string.IsNullOrEmpty(courseCode)) att.CourseCode = courseCode;
                                if (!string.IsNullOrEmpty(sessionId)) att.SessionId = sessionId;
                            }
                            else
                            {
                                var newAtt = new Attendance
                                {
                                    StudentId = matchedStudent.Id,
                                    CourseCode = courseCode ?? string.Empty,
                                    SessionId = sessionId ?? string.Empty,
                                    Date = targetDate,
                                    EntryTime = nowDt,
                                    LastSeenTime = nowDt,
                                    TotalMinutes = 0,
                                    IsPresent = 1.0
                                };
                                dbContext.Attendances.Add(newAtt);
                            }
                            dbContext.SaveChanges();
                        }
                    }

                    var fullRect = new Rect(face.X * 2, face.Y * 2, face.Width * 2, face.Height * 2);
                    Cv2.Rectangle(srcMat, fullRect, Scalar.FromRgb(0, 255, 0), 2);

                    int yLabel = fullRect.Y - 12 > 12 ? fullRect.Y - 12 : fullRect.Y + 15;
                    Cv2.PutText(srcMat, studentName, new Point(fullRect.X, yLabel), HersheyFonts.HersheySimplex, 0.75, Scalar.FromRgb(0, 255, 0), 2);
                }
            }

            Cv2.ImEncode(".jpg", srcMat, out var outBuf);
            return (outBuf, recognizedNames);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FaceRecognitionService ERROR]: {ex.Message}");
            return (frameBytes, recognizedNames);
        }
    }

    public (byte[] ProcessedFrame, int CapturedCount, bool IsComplete) ProcessRegistrationFrame(
        int studentId,
        byte[] frameBytes,
        string datasetPath)
    {
        if (frameBytes == null || frameBytes.Length == 0)
            return (Array.Empty<byte>(), 0, false);

        try
        {
            using var frame = Cv2.ImDecode(frameBytes, ImreadModes.Color);
            if (frame.Empty())
                return (Array.Empty<byte>(), 0, false);

            Rect[] faces = Array.Empty<Rect>();
            if (!_faceClassifier.Empty())
            {
                using var gray = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
                faces = _faceClassifier.DetectMultiScale(gray, 1.1, 4);
            }

            int currentCount = _registrationCounts.TryGetValue(studentId, out var cnt) ? cnt : 0;
            bool isComplete = false;

            if (faces.Length > 0)
            {
                foreach (var face in faces)
                {
                    Cv2.Rectangle(frame, face, Scalar.FromRgb(0, 255, 0), 2);
                }

                if (currentCount < 15)
                {
                    currentCount++;
                    _registrationCounts[studentId] = currentCount;

                    string studentFolder = Path.Combine(datasetPath, studentId.ToString());
                    if (!Directory.Exists(studentFolder))
                    {
                        Directory.CreateDirectory(studentFolder);
                    }

                    string imgPath = Path.Combine(studentFolder, $"{currentCount}.jpg");
                    Cv2.ImWrite(imgPath, frame);
                }

                if (currentCount >= 15)
                {
                    isComplete = true;
                    _registrationCounts.TryRemove(studentId, out _);
                }
            }

            Cv2.ImEncode(".jpg", frame, out var outBuf);
            return (outBuf, currentCount, isComplete);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProcessRegistrationFrame ERROR]: {ex.Message}");
            return (frameBytes, 0, false);
        }
    }
}
