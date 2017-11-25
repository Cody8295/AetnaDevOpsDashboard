$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.logging = true; //debugging
    $.connection.hub.start();
});
function onChange(currentState) {

    console.log("UDPATING"); //debugging

    if (currentState.IsChanged["ProjectGroups"]) {
        console.log(currentState.ProjectGroups.length);
        $(".projectGroups").text(currentState.ProjectGroups.length);
        var pgInside = "";
        for (var pgIndex in currentState.ProjectGroups) {
            var pg = currentState.ProjectGroups[pgIndex];
            pgInside += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + pg.GroupName + "<span class=\"pull-right\">" + pg.ProjectList.count + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
        }
        $(".projectGroupList").html(pgInside);
        // Display an indicator
    }

    if (currentState.IsChanged["Projects"]) {
        console.log("Pj"); //debugging
        $(".projects").text(currentState.Projects.length);
        // Display an indicator
    }

    if (currentState.IsChanged["Lifecycles"]) {
        console.log("Lf"); //debugging
        $(".lifecycles").text(currentState.Lifecycles); // .Count once Lifecycles object is added
        // Display an indicator
    }

    if (currentState.IsChanged["Environments"]) {
        console.log("nE"); //debugging
        $(".numEnvironments").text(currentState.Environments.length);
        var envInside = "";
        for (var envIndex in currentState.Environments) {
            var env = currentState.Environments[envIndex];
            envInside += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + env.Name + "<span class=\"pull-right\">" + env.Description + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
        }
        $(".environments").html(envInside);
        // Display an indicator
    }

    if (currentState.IsChanged["Deploys"]) {
        console.log("dp"); //debugging
        //Update Deploys
        //Display an indicator
    }
   
    
}