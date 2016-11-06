var forms = {
}
var spinner = $("#loading-bar");
var apiDomain = "http://localhost:64097/"; 

function showSingleStats() {
    if(!forms.single.checkValidity()) return false;

    $("#panel-single button").prop("disabled", true);

    var repo = $("#single-repo").val();
    var span = $("#single-span").val();

    $.when(
        $.ajax(apiDomain + "{0}/commits/{1}".f(repo, span)),
        $.ajax(apiDomain + "{0}/contributors/{1}".f(repo, span)),
        $.ajax(apiDomain + "{0}/stargazers/{1}".f(repo, span))
    ).then(
        function(commitsData, contributorsData, stargazersData) {
            commitsData = commitsData[0];
            contributorsData = contributorsData[0];
            stargazersData = stargazersData[0];

            var elements = [
                    [ "Owner:", 
                    "<a href='{0}'>{1}</a>".f(commitsData.owner_url, commitsData.owner_name) 
                    ],
                    [ "Repository:", 
                    "<a href='{0}'>{1}</a>".f(commitsData.url, commitsData.name) 
                    ],
                ];

                function add(a, b) {
                    return a + b;
                }

                elements.push(["Commits:", commitsData.commits.reduce(add, 0)]);
                elements.push(["Contributors:", contributorsData.contributors.reduce(add, 0)]);
                elements.push(["Stars:", stargazersData.stars.reduce(add, 0)]);
                elements.push("Data for {0} days".f(commitsData.span.days.toString()));

                var table = createTable(elements);
                $("#stats-single-info").empty();
                $("#stats-single-info").append(table);
                if (window.chart !== undefined) window.chart.destroy();

                commitsData.contributors = contributorsData.contributors;
                commitsData.stars = stargazersData.stars;

                $("#panel-single button").prop("disabled", false);
                window.chart = createRepoStatsChart(commitsData);
                $("#main-tabs").hide()
                $("#main-tabs").foundation("selectTab", "#stats-single");
        }
    );

    return false;
}

function showTopRepos() {
    if (!forms.repos.checkValidity()) return false;

    var weeks = $("#repos-span").val();
    var count = parseInt($("#repos-count").val());

    $("#panel-repos button, #panel-repos input").prop("disabled", true);

    $.when(
        $.ajax(apiDomain + "repos/commits/{0}".f(weeks)),
        $.ajax(apiDomain + "repos/contributors/{0}".f(weeks)),
        $.ajax(apiDomain + "repos/stargazers/{0}".f(weeks))
    ).then(
        function(a1, a2, a3) {
            var data = [a1, a2, a3];
            data.forEach(function(data) {
                let topReposList = data[0];
                let elements = [];

                let processedItems = 0;
                topReposList.items.some(function(repo) {
                    elements.push(
                        [
                            "<a href='{0}'>{1}</a>".f(repo.url, repo.owner_name 
                                                        + "/" + repo.name),
                            repo.value
                        ]);
                    processedItems++;

                    return processedItems === count;
                });
                
                var tableDivId = "#stats-repos-{0}".f(topReposList.criteria);
                var table = createTable(elements);

                $(tableDivId + ">table").empty();
                $(tableDivId).append(table);
            });

            $("#panel-repos button, #panel-repos input").prop("disabled", false);
            $("#main-tabs").toggle();
            $("#main-tabs").foundation("selectTab", "stats-repos");
        }
    );
    
    return false;
}

function showTopOrgs() {
    if (!forms.repos.checkValidity()) return false;

    $("#panel-orgs button, #panel-orgs input").prop("disabled", true);

    var weeks = $("#orgs-span").val();
    var count = parseInt($("#orgs-count").val());

    $.when(
        $.ajax(apiDomain + "orgs/total/{0}".f(weeks)),
        $.ajax(apiDomain + "orgs/avg/{0}".f(weeks))
    ).then(
        function(a1, a2) {
            var data = [a1, a2];
            data.forEach(function(data) {
                let topOrgsList = data[0];
                let elements = [];

                let processedItems = 0;
                topOrgsList.items.some(function(org) {
                    elements.push(
                        [
                            "<a href='{0}'>{1}</a>".f(org.url, org.name),
                            org.value
                        ]);
                    processedItems++;

                    return processedItems === count;
                });
                
                var tableDivId = "#stats-orgs-{0}".f(topOrgsList.criteria);
                var table = createTable(elements);

                $(tableDivId + ">table").empty();
                $(tableDivId).append(table);
            });

            $("#panel-orgs button, #panel-orgs input").prop("disabled", false);
            $("#main-tabs").toggle();
            $("#main-tabs").foundation("selectTab", "stats-orgs");
        }
    );
    
    return false;
}

function createTable(elements) {
    var table = "<table>";
    var maxcols = 0;
    for(let i = 0; i < elements.length; i++) {
        table += "<tr>";
        if (Object.prototype.toString.call(elements[i]) === "[object Array]") {
            for (let j = 0; j < elements[i].length; j++) {
                table += "<td>" + elements[i][j] + "</td>"; 
            }
            maxcols = elements[i].length > maxcols ? 
                        elements[i].length : maxcols; 
            continue;
        }       
        else {
            table += "<td colspan='{0}'>".f(maxcols.toString())
                         + elements[i] + "</td>";
        } 
        table += "</tr>";
    }
    table += "</table>";
    return table;
}

function createRepoStatsChart(data) {
    var endDate = new Date(data.created_at);
    var dates = [];
    for(let i = 0; i < data.commits.length; i++) {
        let date = new Date(endDate.getTime());
        dates.push(date.toLocaleDateString("en-GB"));
        endDate.setDate(endDate.getDate() - 1);
    }

    dates.reverse();
    var chartData = {
        labels: dates,
        datasets : [
			{   
                label: "Commits",               
				backgroundColor: "rgba(111,84,153,0.5)",
				borderColor : "rgba(111,84,153,0.8)",
				hoverBackgroundColor: "rgba(111,84,153,0.7)",
                hoverBorderColor: "rgba(111,84,153,1)",
				data : data.commits
            },
            {   
                label: "Contributors",               
				backgroundColor: "rgba(238, 172, 6, 0.5)",
				borderColor : "rgba(238, 172, 6, 0.8)",
				hoverBackgroundColor: "rgba(238, 172, 6, 0.7)",
                hoverBorderColor: "rgba(238, 172, 6, 1)",
				data : data.contributors
            },
            {   
                label: "Stars",               
				backgroundColor: "rgba(23, 180, 115, 0.5)",
				borderColor : "rgba(23, 180, 115, 0.8)",
				hoverBackgroundColor: "rgba(23, 180, 115, 0.7)",
                hoverBorderColor: "rgba(23, 180, 115, 1)",
				data : data.stars
            }
        ]
    };

    var ctx = $("#stats-single-chart");
    var myChart = new Chart(ctx, {
        type: 'line',
        data: chartData
    });
}

window.onload = function() {
    forms["single"] = document.getElementById("panel-single").childNodes[1];
    forms["repos"]  = document.getElementById("panel-repos").childNodes[1];
    forms["orgs"]   = document.getElementById("panel-orgs").childNodes[1];

    spinner.hide();

    $(document)
        .ajaxStart(function() {
            $(spinner).show();
        })
        .ajaxStop(function() {
            $(spinner).hide();
        });

    $("#btn-single").click(function() {
        showSingleStats();
        return false;
    });
    $("#btn-repos").click(showTopRepos);
    $("#btn-orgs").click(showTopOrgs);

    $(document).ajaxError(function( event, jqxhr, settings, thrownError ) {
        var errmsg;

        switch(jqxhr.status) {
            case 400: 
                errmsg = "Invalid request.";
                break;
            case 0:
                errmsg = "It looks like service is unavailable at the moment";
                break;
            case 500:
                errmsg = "Internal server error.";
                break;
            default:
                errmsg = "Unexpected error. Status code " + jqxhr.status;
                break; 
        }

        $("#error-message").empty();
        $("#error-message").append(errmsg);

        $("#main-tabs").toggle();
        $("#main-tabs").foundation("selectTab", "error-info");
    });
}

// String format equivalent
String.prototype.format = String.prototype.f = function(){
	var args = arguments;
	return this.replace(/\{(\d+)\}/g, function(m,n){
		return args[n] ? args[n] : m;
	});
};

String.prototype.capitalize = function() {
    return this.charAt(0).toUpperCase() + this.slice(1);
}