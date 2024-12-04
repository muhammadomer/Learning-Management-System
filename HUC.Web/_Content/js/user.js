var baseurl = window.location.href.split('/');
var AttachURL = "/" + baseurl[3];
function pageRefresh() {
    $(window).unload(function () {
        var heatbeatHub = $.connection.heartbeatHub;
        heatbeatHub.server.disconnect();
    });
}

$(document).ready(function () {
    var uiIsDisabled = false;

    function disableUI(showMessage) {
        uiIsDisabled = true;
        $('#content-zone').html('');
        if (showMessage) addError("You appear to be offline please refresh....");
    }

    //client alive beat
    window.setInterval(function () {
        var connectionId = $.connection.hub.id;
        if(connectionId!== undefined)
        {
            debugger;
            var url =  '/Automation/ClientAliveMessage?connectionId=' + connectionId;
            $.ajax({
                cache: false,
                type: 'GET',
                url: url,
                success: function () {
                    debugger;
                console.log("Success");
                },
                error: function () {
                    debugger;
                    console.log(url);
                  console.log("Error");
                }
            });
        }
      
    }, 10000);

    window.setInterval(function () {
        if (uiIsDisabled) return;

        $.ajax({
            cache: false,
            type: 'GET',
            url: AttachURL + '/_Content/IsAlive.txt',
            timeout: 5000,
            success: function () {
                //console.log('server is alive');
            },
            error: function () {
                //console.log('server is down');
                disableUI();
            }
        });
    }, 3000);

    window.addEventListener("offline", function () {
        $.connection.hub.stop();
        disableUI(true);
    }, true);

    var courseHeartbeat = $('#courseHeartbeat').val() === 'true';
    var cID = parseInt($('#cID').val());
    var isTest = $('#isTest').val() === 'true';

   // console.log(courseHeartbeat);
   // console.log(cID);
   // console.log(isTest);

    if (courseHeartbeat) {
       // console.log('beating!');
       
        heartbeat();
        function heartbeat() {
            //console.log('thum-dum');
            debugger;
            var heatbeatHub = $.connection.heartbeatHub;
            //Client response call back....
            heatbeatHub.client.notify = function (response) {
                if (response.ForceClose) {
                    disableUI(false);
                    var heatbeatHub = $.connection.heartbeatHub;
                    heatbeatHub.server.disconnect();
                    return;
                }
            };

            //Disconnect event...
            $.connection.hub.disconnected(function () {
                //var heatbeatHub = $.connection.heartbeatHub;
                //heatbeatHub.server.disconnect();
                disableUI(false);
                //addError("You have disconnected from the server");
            });
            // Start the connection.
            $.connection.hub.start().done(function () {
                // Call the Send method on the hub. 
                debugger;
                var model = {
                    CourseID: cID,
                    IsTest: isTest
                };

                heatbeatHub.server.SendHeartbeatInfo();
                pageRefresh();
            });
        }
    }
});


audiojs.events.ready(function () {
    var as = audiojs.createAll();
});