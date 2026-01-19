let mediaRecorder;
let audioChunks = [];

window.startRecording = async function () {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        mediaRecorder = new MediaRecorder(stream);
        audioChunks = [];

        mediaRecorder.addEventListener("dataavailable", event => {
            audioChunks.push(event.data);
        });

        mediaRecorder.start();
        console.log("Recording started");
    } catch (error) {
        console.error("Error starting recording:", error);
        alert("Could not access microphone. Please check permissions.");
    }
};

window.stopRecording = function () {
    return new Promise((resolve) => {
        if (!mediaRecorder) {
            resolve("");
            return;
        }

        mediaRecorder.addEventListener("stop", async () => {
            const audioBlob = new Blob(audioChunks, { type: 'audio/wav' });
            const reader = new FileReader();

            reader.onloadend = function () {
                const base64data = reader.result.split(',')[1];
                resolve(base64data);
            };

            reader.readAsDataURL(audioBlob);

            // Stop all tracks
            if (mediaRecorder.stream) {
                mediaRecorder.stream.getTracks().forEach(track => track.stop());
            }
        });

        mediaRecorder.stop();
        console.log("Recording stopped");
    });
};

window.playAudio = function (base64Audio) {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        const audioSrc = 'data:audio/wav;base64,' + base64Audio;
        audioPlayer.src = audioSrc;
        audioPlayer.play();
    }
};

// Simple audio visualizer
window.visualizeAudio = function () {
    const canvas = document.getElementById('audioVisualizer');
    if (!canvas || !mediaRecorder || !mediaRecorder.stream) return;

    const audioContext = new (window.AudioContext || window.webkitAudioContext)();
    const analyser = audioContext.createAnalyser();
    const source = audioContext.createMediaStreamSource(mediaRecorder.stream);
    source.connect(analyser);

    analyser.fftSize = 256;
    const bufferLength = analyser.frequencyBinCount;
    const dataArray = new Uint8Array(bufferLength);

    const canvasCtx = canvas.getContext('2d');
    const WIDTH = canvas.width;
    const HEIGHT = canvas.height;

    function draw() {
        if (!mediaRecorder || mediaRecorder.state !== 'recording') return;

        requestAnimationFrame(draw);

        analyser.getByteFrequencyData(dataArray);

        canvasCtx.fillStyle = 'rgb(240, 240, 240)';
        canvasCtx.fillRect(0, 0, WIDTH, HEIGHT);

        const barWidth = (WIDTH / bufferLength) * 2.5;
        let x = 0;

        for (let i = 0; i < bufferLength; i++) {
            const barHeight = (dataArray[i] / 255) * HEIGHT;

            canvasCtx.fillStyle = `rgb(50, ${dataArray[i] + 100}, 200)`;
            canvasCtx.fillRect(x, HEIGHT - barHeight, barWidth, barHeight);

            x += barWidth + 1;
        }
    }

    draw();
};
