function showCalendlyPopup(calendlyUserLink, fullName, email) {
    Calendly.initPopupWidget({
        url: `https://calendly.com/${calendlyUserLink}?hide_gdpr_banner=1`,
        prefill: {
            name: fullName,
            email: email
        },
        utm: {
            utm_source: "BrokerIQ",
            utm_content: email
        },
        text: 'Book client appointment',
        color: '#ffeb3b',
        textColor: '#000000',
        branding: true,
    });
}

function PassPageComponent(dotnetObjectReference) {
    window.pageComponent = dotnetObjectReference;
}

function isCalendlyEvent(e) {
    return e.origin === "https://calendly.com" && e.data.event && e.data.event.indexOf("calendly.") === 0;
};

window.addEventListener("message", function (e) {
    if (isCalendlyEvent(e)) {
        var url = e.data.payload.event.uri;

        console.log(e.data.payload);

        if (url) {
            window.pageComponent.invokeMethodAsync('CreateNewAppointment', url);
        }
    }
});