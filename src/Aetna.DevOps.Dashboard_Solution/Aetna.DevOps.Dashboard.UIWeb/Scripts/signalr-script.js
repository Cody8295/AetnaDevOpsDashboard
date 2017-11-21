$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.logging = true; //debugging
    $.connection.hub.start();
});
function onChange(currentState) {

    console.log("UDPATING"); //debugging

    if (currentState.isChanged["ProjectGroups"]) {
        console.log(currentState.ProjectGroups.length);
        $(".projectGroups").text(currentState.ProjectGroups.length);
        inside = "";
        for (var pg in currentState.ProjectGroups) {
            var pgb = currentState.ProjectGroups[pg];
            inside += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + pgb.groupName + "<span class=\"pull-right\">" + pgb.projectList.count + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
        }
        $(".projectGroupList").html(inside);
        // Display an indicator
    }

    if (currentState.isChanged["Projects"]) {
        console.log("Pj"); //debugging
        $(".projects").text(currentState.Projects.length);
        // Display an indicator
    }

    if (currentState.isChanged["Lifecycles"]) {
        console.log("Lf"); //debugging
        $(".lifecycles").text(currentState.Lifecycles); // .Count once Lifecycles object is added
        // Display an indicator
    }

    if (currentState.isChanged["Environments"]) {
        console.log("nE"); //debugging
        $(".numEnvironments").text(currentState.Environments.length);
        var inside = "";
        for (var env in currentState.Environments) {
            var envb = currentState.Environments[env];
            inside += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + envb.name + "<span class=\"pull-right\">" + envb.description + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
        }
        $(".environments").html(inside);
        // Display an indicator
    }

    if (currentState.isChanged["Deploys"]) {
        console.log("dp"); //debugging
        //Update Deploys
        //Display an indicator
    }
   
    
}