$().ready(function () {
    var $deployHub = $.connection.deployHub;
    $deployHub.client.onChange = onChange;
    $.connection.hub.start();
});
function onChange(currentState) {

    if ($(".projectGroups>span:first").text() != currentState.ProjectGroups) {
        $(".projectGroups").replaceWith("<span class=\"pull-right\">" + currentState.ProjectGroups + "</span>");
        // Display an indicator
    }
    if ($(".projects>span:first").text() != currentState.ProjectGroups) {
        $(".projectGroups").replaceWith("<span class=\"pull-right\">" + currentState.Projects + "</span>");
        // Display an indicator
    }
    if ($(".lifecycles>span:first").text() != currentState.ProjectGroups) {
        $(".projectGroups").replaceWith("<span class=\"pull-right\">" + currentState.Lifecycles + "</span>");
        // Display an indicator
    }
    if ($(".numEnvironments>span:first").text() != currentState.ProjectGroups) {
        $(".projectGroups").replaceWith("<span class=\"pull-right\">" + currentState.Environments + "</span>");
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