$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.logging = true; //debugging
    $.connection.hub.start();
});
function onChange(currentState) {
    console.log("UDPATING"); //debugging
    if (currentState.isChanged["ProjectGroups"]) {
        console.log("PG");
        $(".projectGroups").text(currentState.ProjectGroups);
        // Display an indicator
    }
    if (currentState.isChanged["Projects"]) {
        console.log("Pj"); //debugging
        $(".projects").text(currentState.Projects);
        // Display an indicator
    }
    if (currentState.isChanged["Lifecycles"]) {
        console.log("Lf"); //debugging
        $(".lifecycles").text(currentState.Lifecycles);
        // Display an indicator
    }
    if (currentState.isChanged["Environments"]) {
        console.log("nE"); //debugging
        $(".numEnvironments").text(currentState.Environments);
        // Display an indicator
    }

    /*
    var inside = "";
    response.data.forEach(function (d) {
        inside += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + d.name + "<span class=\"pull-right\">" + d.description + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
    })
    $(".environments").html(inside);

    var inside = "";
    response.data.forEach(function (d) {
        inside += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + d.groupName + "<span class=\"pull-right\">" + d.projectList.count + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
    })
    $(".projectGroupList").html(inside);*/
    
}