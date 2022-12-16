

let onSuccess = function (stream) {
    let recorder;
    let context;
    let audio = document.querySelector('audio');
    let stop = document.querySelector('#StopButton');
    let stopJ = $('#StopButton');
    let record = $('#RecordButton');
    let save = document.querySelector('#SaveButton');
    stop.disabled = false;
    stopJ.removeClass("grey-text");
    stopJ.addClass("blue-text");
    save.disabled = true;
    record.addClass("blink");

    let recording = false;
    let notRecording = true;
    let recorded = false;

    context = new AudioContext();
    let mediaStreamSource = context.createMediaStreamSource(stream);
    recorder = new Recorder(mediaStreamSource);
    recorder.record();

    //visualize(stream, canvas, canvasCtx);


    stop.onclick = function () {
        recorder.stop();
        record.removeClass("blink");
        stopJ.removeClass("blue-text");
        stopJ.addClass("grey-text");

        recorder.exportWAV(function (s) {
            wav = window.URL.createObjectURL(s);
            audio.src = window.URL.createObjectURL(s);
            let filename = new Date().toISOString().replaceAll(':', "");
            var fd = new FormData();
            fd.append("file", s, filename);
            stop.disabled = true;
            save.disabled = false;

            var reader = new window.FileReader();
            reader.readAsDataURL(s);
            reader.onloadend = function () {
                copyS = reader.result;
                copyS = copyS.slice(22);
            }
            // Here

        });

        stop.disabled = true;


        function transferComplete(evt) {
            console.log("The transfer is complete.");
        }

        function transferFailed(evt) {
            console.log("An error occurred while transferring the file.");

            console.log(evt.responseText);
            console.log(evt.status);
        }

    }
}

window.MyJSMethods = {

    startRecording: function () {
        navigator.mediaDevices.getUserMedia({ audio: true })
            .then((stream) => onSuccess(stream))
            .catch((err) => onError(err));
    },



    saveRecording: function () {
        return copyS;
    },

}

var copyS = [];
var pos = 0;

let onError = function (err) {
    alert('We need access to your microphone to record your audio ' + err);
};