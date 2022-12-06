function Assessment(jasonObj) {
    this.jasonObj = jasonObj;
    let length = this.jasonObj.multi.length;
    let label = new Array();
	let radarArray = new Array();
	let selectAnswer = new Array();
    Array(length)
        .fill()
        .map((item, i) => {
            label.push(this.jasonObj.multi[i].tag);
			radarArray.push({
                id: this.jasonObj.multi[i].tag.replace(" ", "-"), 
				score:  0
			});
        })
        .join("");
    let indexing = `${Array(4)
        .fill()
        .map(
            (item, i) => `
     <div class="ev-rangeslider-range-max-${jasonObj.divId} ev-label">
                      <span>${i + 2}</span>
                    </div>`
        )
        .join("")}`;
    
    let choice = `${Array(length)
        .fill()
        .map(
            (item, i) => `
    <div class="ev-component-inner ev-confidenceSlider-inner">
      <div class="ev-display-title ev-confidenceSlider-title ev-component-title">
        <h4 class="ev-component-title-inner" style="text-align: left;">
          ${this.jasonObj.multi[i].title}
        </h4>
      </div>
      <div class="ev-body ev-component-body">
        <div class="ev-component-body-inner">
          <p>
            ${this.jasonObj.multi[i].question}
          </p>
        </div>
      </div>
      <div id="" class="ev-body ev-component-instruction">
        <div class="ev-component-instruction-inner">
          Move the marker to the position you want.
        </div>
      </div>
      <div class="ev-confidenceSlider-widget ev-component-widget ev-clearfix">
        <div class="ev-rangeslider-container">
          <div class="ev-rangeslider-range-labels">
            <div class="ev-rangeslider-range-labels-inner ev-clearfix">
              <div class="ev-rangeslider-range-min ev-label">
                <span>1</span>
              </div>
               <style> .ev-rangeslider-range-max-${jasonObj.divId} { margin-left:${(100 / (this.jasonObj.multi[i].max - 1)) - 2.8}% !important;} </style>
             ${indexing}
            </div>
          </div>
          
          <div class="ev-slider-container">
          <div class="range-wrap" style="width: 100%;">
            <input
              type="range"
              class="range"
              min="1"
              max="${this.jasonObj.multi[i].max}"
              value="${this.jasonObj.multi[i].selected}"
              step="1"
            />
            <output class="bubble"></output>
          </div>
          <div
            class="ev-rangeslider-range-current-value ev-label"
          >
            <span class="ev-angeslider-range-current-value-text"
              >Selected</span
            ><span
              class="ev-rangeslider-range-current-value-number"
              >${this.jasonObj.multi[i].selected}</span
            >
          </div>
        </div>

        </div>
        <div class="ev-buttons ev-buttons-full">
          <div class="ev-buttons-cluster-bottom ev-clearfix">
            <button
              id="${this.jasonObj.multi[i].tag.replace(" ","-")}"
              class="ev-button ev-buttons-action ev-primary hidden"
              aria-label="Select this button to submit your answer"
            >
              Submit
            </button>
          </div>
        </div>
      </div>
    </div>
    `
        )
        .join("")}`;
    let resultTag = `${Array(length)
        .fill()
        .map(
            (item, i) => `<div class="ev-confidence-chart-count-chart-tag">
  <div class="ev-confidence-chart-count-chart-tag-inner">
      <div class="ev-confidence-chart-count-chart-tag-name">
          <h5>${this.jasonObj.multi[i].tag}</h5>
      </div>
      <div class="ev-confidence-chart-count-chart-tag-chart">
          <div id="${this.jasonObj.multi[i].tag.replace(
                /\s/g,
                "-"
            )}" class="ev-confidence-chart-count-chart-tag-chart-item"></div>
      </div>
  </div>
</div>`
        )
        .join("")}`;
    let result = `
   <center>  <button id="ssall" class="ev-button ev-buttons-action ev-primary">Submit All</button></center>
    <div class="ev-block ev-content-row ev-vertical-align-top">
        <div class="ev-block-inner">
            <div class="ev-component-container">
                <div class="ev-component ev-confidenceChart-component ev-component-full">
                    <div class="ev-component-inner ev-confidenceChart-inner">
                        <div class="ev-display-title ev-confidenceChart-title ev-component-title">
                            <h4 class="ev-component-title-inner">Your Results</h4>
                        </div>
                        <div class="ev-body ev-component-body">
                            <div class="ev-component-body-inner">
                                <p>
                                    This radar chart shows the results of your three
                                    confidence answers above. You can download the chart as
                                    a PDF and compare with colleagues. If a colleague isn't
                                    as confident as you in any area why not consider how you
                                    can help them.
                                </p>
                            </div>
                        </div>
                        <div class="ev-confidenceChart-widget ev-component-widget ev-clearfix">
                            <div class="ev-confidence-chart-overview-container-content">
                                <div class="ev-confidence-chart-overview-container-radar">
                                    <div class="chartjs-size-monitor">
                                        <div class="chartjs-size-monitor-expand">
                                            <div class=""></div>
                                        </div>
                                        <div class="chartjs-size-monitor-shrink">
                                            <div class=""></div>
                                        </div>
                                    </div>
                                    
                                    <canvas id="myChart" width="450" height="450" style="display: block; width: 450px; height: 450px;" class="chartjs-render-monitor"></canvas>
                                </div>
                                <div class="ev-confidence-chart-overview-container-breakdown">
                                    ${resultTag}
                                </div>
                            </div>
                            <button class="ev-button hidden">Download Chart</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>`;
    let module = `
<div class="ev-article-inner ev-block-container">
    <div class="ev-block ev-content-row ev-vertical-align-top">
        <div class="ev-block-inner">
            <div class="ev-component-container">
                <div class="ev-component ev-confidenceSlider-component ev-component-full">
                    ${choice}
                </div>
            </div>
        </div>
    </div>
${result}
`;

    this.initialize = () => {
        $(".assessment-module#" + this.jasonObj.divId).append(module);
		
		
        $(document).on("input", (e) => {
            $(e.target)
                .parent()
                .siblings()
                .children(":last-child")
                .text($(e.target).val());
        });
        $("#ssall").on('click', function () {
            $(this).prop("disabled", "true");
            setTimeout(function () {
                $("#Your-Work").trigger("click");
            },500); setTimeout(function () {
                $("#Colleague").trigger("click");
            },500); setTimeout(function () {
                $("#Client").trigger("click");
            },500);
           
           
            $("#Client").trigger("click");
        });
        $("#Client").on("click", (e) => {         
            if ($(e.target).text() != "Download Chart") {
                let assessmentType = $(e.target).attr("id");
				selectAnswer = [];
                $(e.target).prop("disabled", "true");
                disableChoice(e);
                let answerId = $(e.target)
                    .parent()
                    .parent()
                    .siblings()
                    .children(":last-child")
                    .children(":last-child")
                    .children(":last-child")
                    .text();               
                $(
                    ".ev-confidence-chart-count-chart-tag-chart-item#" +
                    $(e.target).attr("id").replace(/\s/g, "-")
                ).addClass("ev-is-complete");
				
				
				var ColleagueId = 0;
				var ClientId = 0;
				var WorkId = 0;
				
				var requestPerameters;               
				let courseId = $("#hdnCourseId").val();
				$.each(radarArray, function (index, value) {
					
					//alert( value.id + ' : ' + value.score );
					if(assessmentType.trim() == value.id)
					{
						value.score = answerId;
					}
					selectAnswer.push(value.score);	

					if (value.id == "Client") {					
						ClientId = value.score;
					}
					else if (value.id == "Colleague") {					
						ColleagueId = value.score;
					}
					else if (value.id == "Your-Work") {					
						WorkId = value.score;
					}
				});
				
				requestPerameters = {
					"id": courseId,
					"ColleagueId": ColleagueId,
					"ClientId": ClientId,
					"WorkId": WorkId
				};
				
				
				
				radarChart(selectAnswer);
                
                
                var ApplicatiionPath = $("#hdnApplicatonPath").val();
                var url = ApplicatiionPath + '/Users/Courses/Assessment';
                
				
				
                AjaxPostRequestWithRequestPerameters(url, requestPerameters, function (response) {
                    if (response.Feedback) {
                        if (response.IsComplete) {
                            console.log("Course Completed");
                        }
                    }
                });
            } else {
                //here to download chart

            }
        });
        $("#Colleague").on("click", (e) => {
            if ($(e.target).text() != "Download Chart") {
                let assessmentType = $(e.target).attr("id");
                selectAnswer = [];
                $(e.target).prop("disabled", "true");
                disableChoice(e);
                let answerId = $(e.target)
                    .parent()
                    .parent()
                    .siblings()
                    .children(":last-child")
                    .children(":last-child")
                    .children(":last-child")
                    .text();
                $(
                    ".ev-confidence-chart-count-chart-tag-chart-item#" +
                    $(e.target).attr("id").replace(/\s/g, "-")
                ).addClass("ev-is-complete");


                var ColleagueId = 0;
                var ClientId = 0;
                var WorkId = 0;

                var requestPerameters;
                let courseId = $("#hdnCourseId").val();
                $.each(radarArray, function (index, value) {

                    //alert( value.id + ' : ' + value.score );
                    if (assessmentType.trim() == value.id) {
                        value.score = answerId;
                    }
                    selectAnswer.push(value.score);

                    if (value.id == "Client") {
                        ClientId = value.score;
                    }
                    else if (value.id == "Colleague") {
                        ColleagueId = value.score;
                    }
                    else if (value.id == "Your-Work") {
                        WorkId = value.score;
                    }
                });

                requestPerameters = {
                    "id": courseId,
                    "ColleagueId": ColleagueId,
                    "ClientId": ClientId,
                    "WorkId": WorkId
                };



                radarChart(selectAnswer);


                var ApplicatiionPath = $("#hdnApplicatonPath").val();
                var url = ApplicatiionPath + '/Users/Courses/Assessment';



                AjaxPostRequestWithRequestPerameters(url, requestPerameters, function (response) {
                    if (response.Feedback) {
                        if (response.IsComplete) {
                            console.log("Course Completed");
                        }
                    }
                });
            } else {
                //here to download chart

            }
        });
        $("#Your-Work").on("click", (e) => {
         
            if ($(e.target).text() != "Download Chart") {
                let assessmentType = $(e.target).attr("id");
                selectAnswer = [];
                $(e.target).prop("disabled", "true");
                disableChoice(e);
                let answerId = $(e.target)
                    .parent()
                    .parent()
                    .siblings()
                    .children(":last-child")
                    .children(":last-child")
                    .children(":last-child")
                    .text();
                $(
                    ".ev-confidence-chart-count-chart-tag-chart-item#" +
                    $(e.target).attr("id").replace(/\s/g, "-")
                ).addClass("ev-is-complete");

               
                var ColleagueId = 0;
                var ClientId = 0;
                var WorkId = 0;

                var requestPerameters;
                let courseId = $("#hdnCourseId").val();
                $.each(radarArray, function (index, value) {

                    //alert( value.id + ' : ' + value.score );
                    if (assessmentType.trim() == value.id) {
                        value.score = answerId;
                    }
                    selectAnswer.push(value.score);

                    if (value.id == "Client") {
                        ClientId = value.score;
                    }
                    else if (value.id == "Colleague") {
                        ColleagueId = value.score;
                    }
                    else if (value.id == "Your-Work") {
                        WorkId = value.score;
                    }
                });

                requestPerameters = {
                    "id": courseId,
                    "ColleagueId": ColleagueId,
                    "ClientId": ClientId,
                    "WorkId": WorkId
                };



                radarChart(selectAnswer);


                var ApplicatiionPath = $("#hdnApplicatonPath").val();
                var url = ApplicatiionPath + '/Users/Courses/Assessment';



                AjaxPostRequestWithRequestPerameters(url, requestPerameters, function (response) {
                    if (response.Feedback) {
                        if (response.IsComplete) {
                            console.log("Course Completed");
                        }
                    }
                });
            } else {
                //here to download chart

            }
        });
        let disableChoice = (e) => {
            $(e.target)
                .parent()
                .parent()
                .siblings()
                .children(":last-child")
                .children(":first-child")
                .children(":first-child")
                .prop("disabled", true);
        };

        let radarChart = (selectAnswer) => {
            const ctx = document.getElementById("myChart");
            const myChart = new Chart(ctx, {
                type: "radar",
                data: {
                    labels: label,
                    datasets: [
                        {
                            data: selectAnswer,
                            backgroundColor: [
                                "rgba(178, 178, 178, 0.5)",
                                "rgba(178, 178, 178,  0.5)",
                                "rgba(178, 178, 178,  0.5)",
                            ],
                            borderColor: [
                                "rgba(102, 204, 153, 1)",
                                "rgba(102, 204, 153, 1)",
                                "rgba(102, 204, 153, 1)",
                            ],
                            borderWidth: 5,
                        },
                    ],
                },
                options: {
					legend: {
						display: false
					},
					tooltips: {
						enabled: false
					},
                    scale: {
                        angleLines: {
                            display: false,
                        },
                         ticks: {
							  beginAtZero: true,
							  min: 0,
							  max: 5,
							  stepSize: 1.0
						},
                    },
                },
            });
        };

		selectAnswer = new Array();
		$.each(radarArray, function (index, value) {
			selectAnswer.push(value.score);
		});
		radarChart(selectAnswer);
    };
	
	
}
