
var inactivityTime = function (value) {
    var time;
    var timeoutInterval = value;
    window.onload = resetTimer;
    // DOM Events
    document.onload = resetTimer;
    document.onmousemove = resetTimer;
    document.onmousedown = resetTimer; // touchscreen presses
    document.ontouchstart = resetTimer;
    document.onclick = resetTimer;     // touchpad clicks
    document.onkeydown = resetTimer;   // onkeypress is deprectaed
    document.addEventListener('scroll', resetTimer, true); // improved; see comments

    function logout() {
        alert("You have been logged out due to user inactivity")
        location.href = 'account/logout'
    }

    function resetTimer() {
        clearTimeout(time);
        time = setTimeout(logout, timeoutInterval); 
    }
}