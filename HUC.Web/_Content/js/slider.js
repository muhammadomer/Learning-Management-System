function Slider(jasonObj) {
   // debugger;
    var questionId = 0;
    var autoTrigger = 0;
    this.jasonObj = jasonObj;
    let length = this.jasonObj.multi.length;
    let remainingAttempts = this.jasonObj.attempts;
    if (this.jasonObj.selected == 0) {
        this.jasonObj.selected = 1;
    }
    
    let selectAnswer = this.jasonObj.selected;
    let answer;
    let popUpMessage1 = "Incorrect, this is a incorrect answer"; //"That is incorrect. Re-read the module then try again.";
    let popUpMessage2 = "Incorrect, this is a incorrect answer";//"That is incorrect and your final attempt";
    let popUpMessage3 = "Well done, this is a correct answer.";
    let attemptsText =
        remainingAttempts == 1 ? "final attempt" : "attempts remaining";
    Array(length)
        .fill()
        .map((item, i) => {
            if (this.jasonObj.multi[i].type == "true") {
                answer = i + 1;
            }
        })
        .join("");
    let choices = `${Array(length)
        .fill()
        .map(
            (item, i) => `
    <p data-ans-id = "${this.jasonObj.multi[i].answerid}">
        ${(i+1) +". " + this.jasonObj.multi[i].option}  
    </p>
    <p><br /></p>`
        )
        .join("")}`;
    let indexing = `${Array(length-1)
        .fill()
        .map(
            (item, i) => `
     <div class="ev-rangeslider-range-max-${jasonObj.divId} ev-label">
                      <span>${i+2}</span>
                    </div>`
        )
        .join("")}`;
    let modal = `
    <div class="modal" id="myModal-${this.jasonObj.divId}">
      <div class="ev-notify">
        <div class="ev-notify-overlay" style="transition: opacity 300ms ease-in 0s; opacity: 1;"></div>
        <div class="ev-notify-prompt-container" style="transform: translateY(0%); opacity: 1; transition: all 300ms ease-in 0s;">
            <div data-focus-guard="true" tabindex="0" style="width: 1px; height: 0px; padding: 0px; overflow: hidden; position: fixed; top: 1px; left: 1px;"></div>
            <div data-focus-guard="true" tabindex="1" style="width: 1px; height: 0px; padding: 0px; overflow: hidden; position: fixed; top: 1px; left: 1px;"></div>
            <div data-focus-lock-disabled="false">
              <div role="dialog" aria-modal="true" aria-describedby="ev-prompt-title-${this.jasonObj.divId}" class="ev-notify-prompt ev-is-incorrect-feedback">
                <div class="ev-notify-content"><div><div class="ev-notify-glyph" style="text-align: center; transition: all 200ms ease-out 200ms; opacity: 1; transform: translateY(0px);">
                  <div class="ev-notify-icon">
                    <i class="ev-icon-cross-circle ev-icon" role="img" aria-label="incorrect"></i>
                  </div>
                </div>
                <h2 id="ev-prompt-title-${this.jasonObj.divId}" class="ev-prompt-title" style="text-align: center;">
                  ${this.jasonObj.title}
                </h2>
                <div id="ev-prompt-body-${this.jasonObj.divId}" class="ev-body ev-prompt-body" style="text-align: center;">
                  <div>
                    <p>
                      ${popUpMessage1}
                    </p>
                  </div>
                </div>
              </div>
            </div>
            <div class="ev-prompt-actions" style="text-align: center;">
              <button type="button" class="ev-button ${this.jasonObj.divId}">
                <span>
                  Close
                </span>
              </button>
            </div>
          </div>
        </div>
        <div data-focus-guard="true" tabindex="0" style="width: 1px; height: 0px; padding: 0px; overflow: hidden; position: fixed; top: 1px; left: 1px;"></div>
      </div>
    </div>
  </div>`;
    let module = `${modal}
<div class="ev-block ev-content-row ev-vertical-align-top">
    <div class="ev-block-inner">
        <div class="ev-component-container">
            <div class="ev-component ev-slider-component ev-component-full">
                <div class="ev-component-inner ev-slider-inner">
                    <div class="ev-display-title ev-slider-title ev-component-title">
                        <h4 class="ev-component-title-inner">${this.jasonObj.title}</h4>
                    </div>
                    <div class="ev-body ev-component-body">
                        <div class="ev-component-body-inner">
                        <p>
                            ${this.jasonObj.question}
                        </p>
                <p><br /></p>
                ${choices}
              </div>
            </div>
            <div id="ev-component-instruction-5eceb13477972b24ae1748b8" class="ev-body ev-component-instruction">
              <div class="ev-component-instruction-inner">
                ${this.jasonObj.subHeading}
              </div>
            </div>
            <div class="ev-slider-widget ev-component-widget ev-clearfix">
              <div class="ev-rangeslider-container">
                <div class="ev-rangeslider-range-labels">
                  <div class="ev-rangeslider-range-labels-inner ev-clearfix">
                    <div class="ev-rangeslider-range-min ev-label">
                      <span>1</span>
                    </div>
                <style> .ev-rangeslider-range-max-${jasonObj.divId} { margin-left:${(100 / (this.jasonObj.max - 1)) - 2.4}% !important;} </style>

                   ${indexing}
                     
                  </div>
                </div>
                <div class="ev-slider-container">
                  <div class="range-wrap" style="width: 100%;">
                    <input type="range" class="range" id="${this.jasonObj.divId}" min="1" max="${this.jasonObj.max}" value="${selectAnswer}" step="1"/>
                    <output class="bubble"></output>
                  </div>
                  <div class="ev-rangeslider-range-current-value ev-label">
                    <span class="ev-angeslider-range-current-value-text"
                      >Selected</span
                    ><span class="ev-rangeslider-range-current-value-number"
                      >${selectAnswer}</span
                    >
                  </div>
                </div>
              </div>
            </div>
            <div class="ev-buttons ev-buttons-full">
              <div class="ev-buttons-display" style="visibility: visible;">
                <div class="ev-buttons-display-inner">
                  <span aria-hidden="false" style="opacity: 1; transform: none;">${remainingAttempts}</span>
                  <span>${attemptsText}</span>
                </div>
              </div>
              <div class="ev-buttons-cluster-bottom ev-clearfix">
                <button
                   type="button" class="ev-button ev-primary ev-buttons-action ${this.jasonObj.divId}"
                  aria-hidden="false"
                  aria-label="Select this button to submit your answer"
                >
                  <span class="ev-button-text">Submit</span></button
                >
                <div class="ev-buttons-marking-icon ev-icon ev-icon-check" style="display: none;"></div>
                <div class="ev-buttons-marking-icon ev-icon ev-icon-cross" style="display: none;"></div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>`;

    this.initialize = () => {
    //    debugger;

        $(".radio-module#" + this.jasonObj.divId).append(module);

        $(document).on("input", "#" + this.jasonObj.divId, (e) => {
            $(e.target)
                .parent()
                .siblings()
                .children(":last-child")
                .text($(e.target).val());
            selectAnswer = $(e.target)
                .parent()
                .siblings()
                .children(":last-child")
                .text();
        });

        //show popup by clicking submit button
        $("button." + this.jasonObj.divId).on("click", (e) => {
            debugger;
            $("#ev-prompt-title-" + this.jasonObj.divId).show();
            $("#ev-prompt-title-" + this.jasonObj.divId).prev().show();
            if ($.trim($(e.target).children().text()) === "Submit") {
                questionId = jasonObj.divId.split("-")[1];
                for (var i = 0; i < jasonObj.multi.length; i++) {
                    if (selectAnswer == i + 1) {
                        $("#hdnQuestionId-" + questionId).val(questionId);
                        $("#hdnAnswerId-" + questionId).val(jasonObj.multi[i].answerid);
                        //$("#hdnAnswerId").val();
                    }
                }
                if (selectAnswer == answer) {
                    $(".submit-answer").removeClass("disabled");
                    // show popup with excellent message.
                    $("#ev-prompt-body-" + this.jasonObj.divId).text(popUpMessage3);
                    $("#ev-prompt-title-" + this.jasonObj.divId)
                        .siblings("div.ev-notify-glyph")
                        .children()
                        .children()
                        .removeClass("ev-icon-cross-circle");
                    $("#ev-prompt-title-" + this.jasonObj.divId)
                        .siblings("div.ev-notify-glyph")
                        .children()
                        .children()
                        .addClass("ev-icon-checkmark-circle");
                    $("#ev-prompt-title-" + this.jasonObj.divId)
                        .siblings("div.ev-notify-glyph")
                        .children()
                        .children()
                        .css("color", "green");
                    $("#myModal-" + this.jasonObj.divId).modal("show");
                    // star icon show on the top of multiple choice.
                    // enable feedback button
                   // enableFeedback(e);
                    // disable submit button
                    disableSubmit(e);
                    // green tick icon shows between feedback button.
                    correctIcon(e);
                    // on parent page disable all choice with correct icon on selected option.
                    disableChoice(e);
                    // hide remaining attempts
                    hideAttempts(e);
                } else {
                    remainingAttempts--;
                    if (remainingAttempts <= 0) {
                        $(".submit-answer").removeClass("disabled");
                        // show popup with message your final attempt
                        $("#ev-prompt-body-" + this.jasonObj.divId).text(popUpMessage2);
                        $("#myModal-" + this.jasonObj.divId).modal("show");
                        // on parent page disable all choice with cross icon on selected option.
                        // incorrectAnswer(e);
                        // hide remaining attempts
                        hideAttempts(e);
                        // button change to Show Answer
                        enableShowAnswer(e);
                        // red cross icon show between feedback button
                        incorrectIcon(e);
                        //enable feedback button.
                        
                        //disable all multiple choice.
                        disableChoice(e);
                    } else if (remainingAttempts == 1) {
                        // main page change text remaining attempts to final attempts
                        $("#myModal-" + this.jasonObj.divId).modal("show");
                        /*on parent page same as else part except failed popup
                          remaining attempts value decrease.*/
                        decrementAttempts(e);
                        changeAttemptsText(e);
                        // show reset button.
                        enableReset(e);
                        // enable feedback button.
                       
                        //disable all multiple choice.
                        disableChoice(e);
                    } else { 
                        // failed popup
                        $("#ev-prompt-body-" + this.jasonObj.divId).text(popUpMessage2);
                        $("#myModal-" + this.jasonObj.divId).modal("show");
                        // remaining attempts value decrease.
                        decrementAttempts(e);
                        // show reset button.
                        enableReset(e);
                        // enable feedback button.
                        
                        // disable all multiple choice.
                        disableChoice(e);
                    }
                }
            }
            else if ($.trim($(e.target).children().text()) == "Reset") {
                // change reset button to submit button
                $(e.target).children().text("Submit");
                // disable submit button
                $(e.target).prop("disabled", false);
                // disable feedback button
               
                // enable All multiple choice
                enableChoice(e);
            }
            else if ($.trim($(e.target).children().text()) == "Close") {
                // $("#submitAnswer" + questionId).submit();
                if (autoTrigger == 1) {
                    autoTrigger = 0;
                }
                else {
                    var ModuleId = $("#hdnModuleId").val();
                    var CourseId = $("#hdnCourseId-" + questionId).val();
                    var QuestionId = $("#hdnQuestionId-" + questionId).val();
                    var AnswerId = $("#hdnAnswerId-" + questionId).val();

                  //  debugger;

 var freetext = "";
                    var ApplicatiionPath = $("#hdnApplicatonPath").val();
                    var url = ApplicatiionPath + '/Users/Courses/SubmitAnswer';
                    var requestPerameters = {
                        "id": ModuleId,
                        "course": CourseId,
                        "answer": AnswerId,
                        "question": QuestionId,
                        "flagged": false,
"FreeTextAnswer": freetext
                    }
                    AjaxPostRequestWithRequestPerameters(url, requestPerameters, function (response) {
                        if (response.Feedback) {
                            if (response.IsComplete) {
                                console.log("Alhumdulillah");
                                var nextModuleId = $("#hdnNextModuleId").val();
                                var courseId = $("#hdnCourseId").val();
                                var nextModuleURL = $("#hdnApplicatonPath").val() +"/Users/Courses/ModuleDetail/" + nextModuleId;
                                if (nextModuleId == 0) {
                                    nextModuleId = $("#hdnCourseId").val();
                                    nextModuleURL = $("#hdnApplicatonPath").val() + "/Users/Courses/Assessment/" + nextModuleId;
                                }
                                $(".disabled-link").find("a").attr("href", nextModuleURL);
                                $(".disabled-link").removeClass("disabled-link");
                            }
                        }
                    });
                    $("#myModal-" + this.jasonObj.divId).modal("hide");
                }
            }
        
            else if ($.trim($(e.target).children().text()) == "Show Answer") {
                showCorrectAnswer(e);
            }
            else if ($.trim($(e.target).children().text()) == "Hide Answer") {
                hideCorrectAnswer(e);
            }
        });
        if (this.jasonObj.selectedAnswer > 0) {
            autoTrigger = 1;
            $("button." + this.jasonObj.divId).trigger("click");
            $("#myModal-" + this.jasonObj.divId).modal("hide");
        }
    };
    let decrementAttempts = (e) => {
        $(e.target)
            .parent()
            .siblings()
            .children()
            .children("span:first-child")
            .text(remainingAttempts);
    };

    let changeAttemptsText = (e) => {
        $(e.target)
            .parent()
            .siblings()
            .children()
            .children("span:last-child")
            .text("final attempt");
    };

    let enableReset = (e) => {
        $(e.target).children().text("Reset");
    };

    let enableHideAnswer = (e) => {
        $(e.target).children().text("Hide Answer");
    };

    let enableShowAnswer = (e) => {
        $(e.target).children().text("Show Answer");
    };

    let hideCorrectAnswer = (e) => {
        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-slider-widget")
            .children()
            .children(":last-child")
            .children(":first-child")
            .children(":first-child")
            .val(selectAnswer);

        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-slider-widget")
            .children()
            .children(":last-child")
            .children(":last-child")
            .children(":last-child")
            .text(selectAnswer);
        enableShowAnswer(e);
    };

    let showCorrectAnswer = (e) => {
        selectAnswer = $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-slider-widget")
            .children()
            .children(":last-child")
            .children(":last-child")
            .children(":last-child")
            .text();

        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-slider-widget")
            .children()
            .children(":last-child")
            .children(":first-child")
            .children(":first-child")
            .val(answer);

        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-slider-widget")
            .children()
            .children(":last-child")
            .children(":last-child")
            .children(":last-child")
            .text(answer);
        enableHideAnswer(e);
    };

    let disableChoice = (e) => {
        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-slider-widget")
            .children()
            .children(":last-child")
            .children(":first-child")
            .children()
            .prop("disabled", true);
    };

    let enableChoice = (e) => {
        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-slider-widget")
            .children()
            .children(":last-child")
            .children(":first-child")
            .children()
            .prop("disabled", false);
        // defualt radio vale
        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-slider-widget")
            .children()
            .children(":last-child")
            .children(":first-child")
            .children(":first-child")
            .val(this.jasonObj.selected);

        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-slider-widget")
            .children()
            .children(":last-child")
            .children(":last-child")
            .children(":last-child")
            .text(this.jasonObj.selected);
    };

    let hideAttempts = (e) => {
        $(e.target).parent().siblings().css("visibility", "hidden");
    };

    let disableSubmit = (e) => {
        $(e.target).prop("disabled", true);
    };





    let incorrectIcon = (e) => {
        $(e.target).siblings("div.ev-icon-cross").css("display", "block");
    };

    let correctIcon = (e) => {
        $(e.target).siblings("div.ev-icon-check").css("display", "block");
    };
}
