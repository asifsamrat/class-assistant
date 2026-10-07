window.cameraInterop = {
    videoElement: null,
    canvasElement: null,
    canvasCtx: null,
    stream: null,
    intervalId: null,
    hubConnection: null,

    startCamera: async function (videoDeviceId, hubConn) {
        try {
            this.hubConnection = hubConn;
            const constraints = {
                video: videoDeviceId ? { deviceId: { exact: videoDeviceId } } : { width: { ideal: 640 }, height: { ideal: 480 } }
            };

            this.stream = await navigator.mediaDevices.getUserMedia(constraints);
            
            if (!this.videoElement) {
                this.videoElement = document.createElement('video');
                this.videoElement.autoplay = true;
                this.videoElement.playsInline = true;
            }
            this.videoElement.srcObject = this.stream;
            await this.videoElement.play();

            if (!this.canvasElement) {
                this.canvasElement = document.createElement('canvas');
            }
            this.canvasCtx = this.canvasElement.getContext('2d');

            return true;
        } catch (err) {
            console.error("Error starting camera:", err);
            return false;
        }
    },

    captureAndSendFrame: function (dotNetHelper, hubConnection, courseCode, sessionId, selectedDate, isRegistration, studentId) {
        if (this.intervalId) clearInterval(this.intervalId);

        this.intervalId = setInterval(() => {
            if (!this.videoElement || this.videoElement.paused || this.videoElement.ended) return;

            const width = this.videoElement.videoWidth || 640;
            const height = this.videoElement.videoHeight || 480;

            this.canvasElement.width = width * 0.5;
            this.canvasElement.height = height * 0.5;

            this.canvasCtx.drawImage(this.videoElement, 0, 0, this.canvasElement.width, this.canvasElement.height);

            this.canvasElement.toBlob(async (blob) => {
                if (!blob) return;
                const buffer = await blob.arrayBuffer();
                const bytes = new Uint8Array(buffer);

                if (hubConnection && hubConnection.state === "Connected") {
                    if (isRegistration && studentId) {
                        hubConnection.invoke("RegisterCaptureFrame", parseInt(studentId), Array.from(bytes));
                    } else {
                        hubConnection.invoke("SendClientFrame", Array.from(bytes));
                    }
                }
            }, 'image/jpeg', 0.6);
        }, 150);
    },

    stopCamera: function () {
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }
        if (this.stream) {
            this.stream.getTracks().forEach(track => track.stop());
            this.stream = null;
        }
        if (this.videoElement) {
            this.videoElement.srcObject = null;
        }
    },

    renderFrameToCanvas: function (canvasId, frameBase64) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');
        const img = new Image();
        img.onload = () => {
            canvas.width = img.width;
            canvas.height = img.height;
            ctx.drawImage(img, 0, 0);
        };
        img.src = frameBase64.startsWith('data:image') ? frameBase64 : `data:image/jpeg;base64,${frameBase64}`;
    }
};
