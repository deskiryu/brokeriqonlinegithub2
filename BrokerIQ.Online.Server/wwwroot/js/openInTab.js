
function openInTab(base64EncodedPDF, jpeg) {
    openInTabBase(base64EncodedPDF, false);
}

function openInTabJpeg(base64EncodedPDF, jpeg) {
    openInTabBase(base64EncodedPDF, true);
}

function openInTabBase(base64EncodedPDF, jpeg) {

    var base64str = base64EncodedPDF;

    // decode base64 string, remove space for IE compatibility
    var binary = atob(base64str.replace(/\s/g, ''));
    var len = binary.length;
    var buffer = new ArrayBuffer(len);
    var view = new Uint8Array(buffer);
    for (var i = 0; i < len; i++) {
        view[i] = binary.charCodeAt(i);
    }
    // create the blob object with content-type "application/pdf"               
    var blob = new Blob([view], { type: jpeg ? "image/jpeg" : "application/pdf" });
    var url = URL.createObjectURL(blob);
    window.open(url);

}

function changeTabIcon(iconLocation) {
    document.getElementById('favicon').href = iconLocation;
}

