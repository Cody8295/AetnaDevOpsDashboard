$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.logging = true; //debugging
    $.connection.hub.start();
});
function onChange(currentState) {
    console.log("HEY! I'M DOING IT"); //debugging
    if ($(".projectGroups").first().text() !== currentState.ProjectGroups) {
        console.log($(".projectGroups").first().children().first().html()); //debugging
        $(".projectGroups").replaceWith("<span class=\"pull-right\">Hi" + currentState.ProjectGroups + "</span>");
        // Display an indicator
    } else {
        console.log($(".projectGroups").first().text().toString()); //debugging
    }
    if ($(".projects").innerHTML != currentState.Projects) {
        console.log("Pj"); //debugging
        $(".projects").replaceWith("<span class=\"pull-right\">" + currentState.Projects + "</span>");
        // Display an indicator
    }
    if ($(".lifecycles").innerHTML != currentState.Lifecycles) {
        console.log("Lf"); //debugging
        $(".lifecycles").replaceWith("<span class=\"pull-right\">" + currentState.Lifecycles + "</span>");
        // Display an indicator
    }
    if ($(".numEnvironments").innerHTML != currentState.Environments) {
        console.log("nE"); //debugging
        $(".numEnvironments").replaceWith("<span class=\"pull-right\">" + currentState.Environments + "</span>");
        // Display an indicator
    }

    /*
    var replace = "<div id=\"environments\" class=\"collapsible panel-collapse collapse\">";
    response.data.forEach(function (d) {
        replace += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + d.name + "<span class=\"pull-right\">" + d.description + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
    })
    replace += "</div>";
    $(".environments").replaceWith(replace);

    var replace = "<div id=\"projectGroupList\" class=\"collapsible panel-collapse collapse\">";
    response.data.forEach(function (d) {
        replace += "<div class=\"panel- footer\">&nbsp;&nbsp;&nbsp;" + d.groupName + "<span class=\"pull-right\">" + d.projectList.count + "&nbsp;&nbsp;&nbsp;&nbsp;</span></div>";
    })
    $(".projectGroupList").replaceWith(replace);*/
    
}