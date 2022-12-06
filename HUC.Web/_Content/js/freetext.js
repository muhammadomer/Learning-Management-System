function FreeText(jasonObj) {
    
    var questionId = 0;
    var autoTrigger = 0;
    this.jasonObj = jasonObj;
    let remainingAttempts = this.jasonObj.attempts;
    let title = this.jasonObj.title;
    let question = this.jasonObj.question;
    let length = this.jasonObj.multi.length;
    let statement = this.jasonObj.statement;
    let adminpreviewAns = this.jasonObj.adminpreviewAns;
    let freetextCount = this.jasonObj.freetextCount;
    let filterSelectAnswer;
    let answer;
    let btnSubmit;
    let siblingsOfRadio;
    let siblingsOfLabel;
    let selectRadioIcon;
    let highlightLabel;
    let popUpMessage1 = "Incorrect, this is a incorrect answer"; //"That is incorrect. Re-read the module then try again.";
    let popUpMessage2 = "Incorrect, this is a incorrect answer";//"That is incorrect and your final attempt";
    let popUpMessage3 = "Your answer will be reviewed by course admin";
    let selectAnswer;
    let attemptsText =
        remainingAttempts == 1 ? "final attempt" : "attempts remaining";
    let adminpreviewTxt = "<p style='display:none;color:red'>Waiting for admin to preview</p>";
    Array(length)
        .fill()
        .map((item, i) => {
            if (this.jasonObj.multi[i].type == "true") {
                answer = this.jasonObj.multi[i].option;
            }
        })
        .join("");
    let choices = `${Array(length)
        .fill()
        .map(
            (item, i) => `
    <div class="ev-grid-item" style="flex: 1 1 100%; text-align: center; box-sizing: border-box; margin: 10px 0px;">
      <div class="ev-grid-item-inner" style="padding: 0px 10px; box-sizing: border-box;">
        <div class="ev-mcq-item ev-component-item ev-component-item-color">
          <input id="${this.jasonObj.divId + (i + 1)}" class="${this.jasonObj.divId
                }" name="" data-ans-id = "${this.jasonObj.multi[i].answerid}" type="radio" value="0"/>
          <label for="${this.jasonObj.divId + (i + 1)
                }" class="ev-component-item-text-color ev-component-item-border">
            <div class="ev-mcq-item-state ev-component-item-state">
              <div class="ev-mcq-item-icon ev-mcq-answer-icon ev-component-item-text-color ev-icon ev-icon-circle">
              </div>
            </div>
            <span class="ev-mcq-item-inner ev-component-item-inner">
              ${this.jasonObj.multi[i].option}                          
            </span>
          </label>
        </div>
      </div>
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
                    <i class="ec" role="img" aria-label="incorrect"><img src="/TrainingCourses/_Content/fonts/reistered.svg" height="180" width="170" style="max-width:88%;margin-bottom:10px" /></i>
                  </div>
                </div>
                <h2 id="ev-prompt-title-${this.jasonObj.divId}" class="ev-prompt-title" style="text-align: center;">
                  ${title}
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
                  <div class="ev-component ev-mcq-component ev-component-full">
                      <div class="ev-component-inner ev-mcq-inner">
                          <div class="ev-display-title ev-mcq-title ev-component-title">
                              <h4 class="ev-component-title-inner">
                                  ${title}
                              </h4>
                          </div>
                          <div class="ev-body ev-component-body">
                              <div class="ev-component-body-inner">
                              <p>
                                  ${question}
                              </p>
                          </div>
                      </div>
                      <div class="ev-body ev-component-instruction">
                          <div class="ev-component-instruction-inner">
                              ${statement} ${adminpreviewTxt}
                          </div>
                      </div>
                      <div class="ev-component-interaction-label ev-is-incomplete" style="display: block;">
                          <div class="ev-interaction-label-inner">
                              <i class="ev-icon ev-interaction-label-icon ev-icon-mouse-left">  </i>
                         
                          </div>
                      </div>
                      <div class="ev-component-interaction-label ev-is-complete" style="display: none;">
                          <div class="ev-interaction-label-inner">
                              <i class="ev-icon ev-interaction-label-icon ev-icon-star"></i>
                            
                          </div>
                      </div>
                      <div class="ev-mcq-widget ev-component-widget ev-clearfix">
                          <div class="ev-mcq-items" role="radiogroup">
                              <div class="ev-grid ev-column-layout-1">
                                  ${choices}
                              </div>
                          </div>
                      </div>
                      <div class="ev-buttons ev-buttons-full">
                          <div class="ev-buttons-display" style="visibility: visible;">
                              <div class="ev-buttons-display-inner">
                                  <span aria-hidden="false" style="opacity: 1; transform: none;">
                                      ${remainingAttempts}
                                  </span>
                                  <span> 
                                      ${attemptsText}
                                  </span>
                              </div>
                          </div>
                          <div class="ev-buttons-cluster-bottom ev-clearfix">
                              <button id="submitbtnfreetext-${this.jasonObj.questionId}" type="button" class="ev-button ev-primary ev-buttons-action ${this.jasonObj.divId}" data-question-id = "${this.jasonObj.divId.split('-')[1]}" aria-hidden="true" aria-label="Select this button to submit your answer">
                                  <span class="ev-button-text">
                                      Submit
                                  </span>
                              </button>
                              <button type="button" disabled="" class="ev-button ev-primary ev-buttons-feedback ev-button-icon-before ${this.jasonObj.divId}" aria-label="Select this button to show the feedback">
                                  <span class="ev-button-text">
                                      No Feedback
                                  </span>
                              </button>
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
       
        $(".freetext-module#" + this.jasonObj.divId).append(module);
        $("#freetext").on("click", (e) => {
           
            var ansId = $(e.target).attr("data-ans-id");
            questionId = $(e.target).attr("class").split('-')[1];
            $("#hdnQuestionId-" + questionId).val(questionId);
            $("#hdnAnswerId-" + questionId).val(ansId);
            siblingsOfRadio = $(e.target)
                .parent()
                .parent()
                .parent()
                .siblings()
                .children()
                .children()
                .children("label")
                .children("div")
                .children();

            siblingsOfLabel = $(e.target)
                .parent()
                .parent()
                .parent()
                .siblings()
                .children()
                .children()
                .children("label");

            //remove siblings radio icon
            if (siblingsOfRadio.hasClass("ev-icon-radio-button"))
                siblingsOfRadio.removeClass("ev-icon-radio-button");

            //remove highlighted siblings
            if (siblingsOfLabel.hasClass("ev-selected"))
                siblingsOfLabel.removeClass("ev-selected");

            //highlight radio button
            highlightLabel = $(e.target).siblings("label").addClass("ev-selected");
            selectRadioIcon = $(e.target)
                .siblings()
                .children("div")
                .children()
                .addClass("ev-icon-radio-button");

            //store selected answer
            selectAnswer = $(e.target).siblings().children("span");
            filterSelectAnswer = $.trim(selectAnswer.text());

            // enable submit button
            btnSubmit = $(e.target)
                .parent()
                .parent()
                .parent()
                .parent()
                .parent()
                .parent()
                .siblings("div.ev-buttons")
                .children("div.ev-buttons-cluster-bottom")
                .children("button.ev-buttons-action");
            btnSubmit.prop("disabled", false);
        });
        debugger;
        if (adminpreviewAns == 1) {
            //showAdminPreviewText();
            $('#freetext-' + this.jasonObj.divId.split('-')[1]).parent().find('p').show();
        }
      


        //show popup by clicking submit button
        $("button." + this.jasonObj.divId).on("click", (e) => {
         
        
          
            if ($.trim($(e.target).children().text()) === "Submit") {
                $('#freetext-' + this.jasonObj.divId.split('-')[1]).parent().find('p').show();
                questionId = $(e.target).attr("data-question-id")
                if (filterSelectAnswer == answer) {
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
                        .addClass("ev-ico");//Naqi 
                    $("#ev-prompt-title-" + this.jasonObj.divId)
                        .siblings("div.ev-notify-glyph")
                        .children()
                        .children()
                        .css("color", "green");
                    $("#myModal-" + this.jasonObj.divId).modal("show");

                    // star icon show on the top of multiple choice.
                    starIcon(e);
                    // enable feedback button
                  
                    // disable submit button
                    disableSubmit();
                    // green tick icon shows between feedback button.
                    correctIcon(e);
                    // on parent page disable all choice with correct icon on selected option.
                    correctAnswer(e);
                    // hide remaining attempts
                    hideAttempts(e);
                } else {
                    remainingAttempts--;
                   
                    if (remainingAttempts <= 0) {
                        // show popup with message your final attempt
                        $("#ev-prompt-body-" + this.jasonObj.divId).text(popUpMessage2);
                        $("#myModal-" + this.jasonObj.divId).modal("show");
                        // on parent page disable all choice with cross icon on selected option.
                        incorrectAnswer(e);
                        // hide remaining attempts
                        hideAttempts(e);
                        // star icon show on the top of multiple choice.
                        starIcon(e);
                        // button change to Show Answer
                        enableShowAnswer(e);
                        // red cross icon show between feedback button
                        incorrectIcon(e);
                        //enable feedback button.
                        enableFeedback(e);
                    }
                    else if (remainingAttempts == 1) {
                        // main page change text remaining attempts to final attempts
                        $("#myModal-" + this.jasonObj.divId).modal("show");
                        /*on parent page same as else part except failed popup
                          remaining attempts value decrease.*/
                        decrementAttempts(e);
                        changeAttemptsText(e);
                        // show reset button.
                        enableReset();
                        // enable feedback button.
                        enableFeedback(e);
                        //disable all multiple choice.
                        disableChoice(e);
                       
                    }
                    else {
                        // failed popup
                        $("#myModal-" + this.jasonObj.divId).modal("show");
                        // remaining attempts value decrease.
                        decrementAttempts(e);
                        // show reset button.
                        enableReset();
                        // enable feedback button.
                        enableFeedback(e);
                        // disable all multiple choice.
                        disableChoice(e);
                       
                    }
                }
            }
            else if ($.trim($(e.target).children().text()) == "Reset") {
                // change reset button to submit button
                btnSubmit.children().text("Submit");
                // disable submit button
                btnSubmit.prop("disabled", true);
                // disable feedback button
                disableFeedback(e);
                // enable All multiple choice
                enableChoice(e);
            }
            else if ($.trim($(e.target).children().text()) == "Close") {
               // debugger;
                //int id, int course, IEnumerable<int> answer, int question = 0, bool flagged = false
                //$("#submitAnswer" + questionId).submit();
                if (autoTrigger == 1) {
                    autoTrigger = 0;
                }
                else {
                    var ModuleId = $("#hdnModuleId").val();
                    var CourseId = $("#hdnCourseId-" + questionId).val();
                    //var QuestionId = $("#hdnQuestionId-" + questionId).val();
                    var QuestionId = $("#freetextqt-" + questionId).val();
                    var AnswerId = $("#hdnAnswerId-" + questionId).val();
                    var ApplicatiionPath = $("#hdnApplicatonPath").val();

                    var freetext = $("#freetext-" + questionId).val();
                    console.log(freetext);
                    var requestPerameters = {
                        "id": ModuleId,
                        "course": CourseId,
                        "answer": AnswerId,
                        "question": QuestionId,
                        "flagged": false,
                        "FreeTextAnswer": freetext
                    }
                    var url = ApplicatiionPath + '/Users/Courses/SubmitAnswer';
                    AjaxPostRequestWithRequestPerameters(url, requestPerameters, function (response) {
                      //  debugger;
                        console.log(response);
                        if (response.Feedback) {
                            if (response.IsComplete) {
                                console.log("Alhumdulillah");
                                $("#freetext -"+ questionId).attr("disabled", "disabled"); 
                                var nextModuleId = $("#hdnNextModuleId").val();
                                var nextModuleURL = $("#hdnApplicatonPath").val() + "/Users/Courses/ModuleDetail/" + nextModuleId;
                                if (nextModuleId == 0) {
                                    nextModuleId = $("#hdnCourseId").val();
                                    nextModuleURL = $("#hdnApplicatonPath").val() + "/Users/Courses/Assessment/" + nextModuleId;
                                }
                                $(".disabled-link").find("a").attr("href", nextModuleURL);
                                $(".disabled-link").removeClass("disabled-link");
                            }
                        }
                      //  $("button." + this.jasonObj.divId).prop("disabled", true);
                        $("button." + "module-" + questionId).prop("disabled", true);
                        $("#freetext-"+ questionId).attr("disabled", "disabled");  
                    });
                    $("#myModal-" + this.jasonObj.divId).modal("hide");
                }
            }
            else if ($.trim($(e.target).children().text()) == "Show Feedback") {

                $("#ev-prompt-body-" + this.jasonObj.divId).empty();
                $("#ev-prompt-body-" + this.jasonObj.divId).append(jasonObj.feedback);
                enableSimpleFeedback(e);
                $("#myModal-" + this.jasonObj.divId).modal("show");
            }
            else if ($.trim($(e.target).children().text()) == "Show Answer") {
                showCorrectAnswer(e);
            } else if ($.trim($(e.target).children().text()) == "Hide Answer") {
                hideCorrectAnswer(e);
            }
        });
        Array(length)
            .fill()
            .map((item, i) => {
                if (this.jasonObj.multi[i].selected == "true") {
                    autoTrigger = 1;
                    //select Radio Button
                    $("input#" + this.jasonObj.divId + (i + 1)).trigger("click");
                    //click Submit
                    $("button." + this.jasonObj.divId).trigger("click");
                    $("#myModal-" + this.jasonObj.divId).modal("hide");

                }
            })
            .join("");
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

    let enableReset = () => {
        btnSubmit.children().text("Reset");
    };

    let enableHideAnswer = () => {
        btnSubmit.children().text("Hide Answer");
    };

    //let enableShowAnswer = () => {
    //    btnSubmit.children().text("Show Answer");
    //};

    let hideCorrectAnswer = (e) => {
        let findAnswer = $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-mcq-widget")
            .children()
            .children()
            .children()
            .children()
            .children()
            .children("label")
            .children("span:contains(" + answer + ")");
        // enable icon and selected
        findAnswer.parent().addClass("ev-disabled");
        findAnswer.parent().removeClass("ev-selected");
        findAnswer.siblings().children().remove();
        selectAnswer
            .siblings()
            .html(
                '<div class="ev-mcq-item-icon ev-mcq-answer-icon ev-component-item-text-color ev-icon ev-icon-cross"></div>'
            );
        selectAnswer.parent().addClass("ev-selected");
        selectAnswer.parent().removeClass("ev-disabled");
        enableShowAnswer();
    };

    let showCorrectAnswer = (e) => {
        let findAnswer = $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-mcq-widget")
            .children()
            .children()
            .children()
            .children()
            .children()
            .children("label")
            .children("span:contains(" + answer + ")");
        // enable icon and selected
        findAnswer.parent().removeClass("ev-disabled");
        findAnswer.parent().addClass("ev-selected");
        findAnswer
            .siblings()
            .html(
                '<div class="ev-mcq-item-icon ev-mcq-answer-icon ev-component-item-text-color ev-icon ev-icon-check"></div>'
            );
        selectAnswer.siblings().children().remove();
        selectAnswer.parent().removeClass("ev-selected");
        selectAnswer.parent().addClass("ev-disabled");
        enableHideAnswer();
    };

    let correctAnswer = (e) => {
        // remove siblings radio icons
        siblingsOfRadio.remove();

        // change green check icon
        selectRadioIcon
            .removeClass("ev-icon-circle")
            .removeClass("ev-icon-radio-button");
        selectRadioIcon.addClass("ev-icon-check");

        /* disable other choice
          disable all input type radio */
        disableChoice(e);
    };

    let incorrectAnswer = (e) => {
        // remove siblings radio icons
        siblingsOfRadio.remove();

        // change red cross icon
        selectRadioIcon
            .removeClass("ev-icon-circle")
            .removeClass("ev-icon-radio-button");
        selectRadioIcon.addClass("ev-icon-cross");

        /* disable other choice
          disable all input type radio */
        disableChoice(e);
    };

    let disableChoice = (e) => {
        siblingsOfLabel.addClass("ev-disabled");
        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-mcq-widget")
            .children()
            .children()
            .children()
            .children()
            .children()
            .children("input")
            .prop("disabled", true);
    };

    let enableChoice = (e) => {
        siblingsOfLabel.removeClass("ev-disabled");
        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-mcq-widget")
            .children()
            .children()
            .children()
            .children()
            .children()
            .children("input")
            .prop("disabled", false);
        highlightLabel.removeClass("ev-selected");
        selectRadioIcon.removeClass("ev-icon-radio-button");
    };

    let hideAttempts = (e) => {
        $(e.target).parent().siblings().css("visibility", "hidden");
    };

    let disableSubmit = () => {
        btnSubmit.prop("disabled", true);
    };

    let enableFeedback = (e) => {
        $(e.target).siblings("button.ev-buttons-feedback").prop("disabled", false);
    };

 

    let disableFeedback = (e) => {
        $(e.target).siblings("button.ev-buttons-feedback").prop("disabled", true);
    };

    let starIcon = (e) => {
        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-is-complete")
            .css("display", "block");
        $(e.target)
            .parent()
            .parent()
            .siblings("div.ev-is-incomplete")
            .css("display", "none");

      
    };

    let incorrectIcon = (e) => {
        $(e.target).siblings("div.ev-icon-cross").css("display", "block");
    };

    let correctIcon = (e) => {
        $(e.target).siblings("div.ev-icon-check").css("display", "block");
    };

    let showAdminPreviewText = (e) => {
        $(e.target)
            .parent()
            .siblings()
            .children()
            .children("span:first-child")
            .text(adminpreviewTxt)
            .css('display', 'block');
    };

}
