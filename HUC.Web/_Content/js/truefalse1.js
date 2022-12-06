function TrueFalse(jasonObj) {
    this.jasonObj = jasonObj;
    var autoTrigger = 0;
    let remainingAttempts = this.jasonObj.attempts;
    let title = this.jasonObj.title;
    let length = this.jasonObj.question.length;
    let statement = this.jasonObj.statement;
    let trueAnswer = new Array();
    let falseAnswer = new Array();
    let trueAnswerList = new Array();
    let falseAnswerList = new Array();
    let saveTrueList;
    let saveFalseList;
    let btnSubmit;
    let popUpMessage1 = "Incorrect, this is a incorrect answer"; //"That is incorrect. Re-read the module then try again.";
    let popUpMessage2 = "Incorrect, this is a incorrect answer";//"That is incorrect and your final attempt";
    let popUpMessage3 = "Well done, this is a correct answer.";
    let attemptsText =
        remainingAttempts == 1 ? " final attempt" : " attempts remaining";
    Array(length)
        .fill()
        .map((item, i) => {
            if (this.jasonObj.question[i].type === "true") {
                trueAnswer.push(this.jasonObj.question[i].option);
            } else {
                falseAnswer.push(this.jasonObj.question[i].option);
            }
        })
        .join("");
    let questionLength;
    let choices = `${Array(length)
        .fill()
        .map(
            (item, i) => `
      <button
      draggable="true"
      id="${this.jasonObj.divId}-dragtarget${[i]}"
      class="ev-draggable-item ev-component-item ${this.jasonObj.divId
                }-dragtarget"
      style="left: 0px; top: 0px; width: 100%;"
    >
      <i
        class="ev-icon ev-draggable-item-icon ev-icon-move"
      ></i
      ><span data-ans-id = "${this.jasonObj.question[i].answerid}" id="${this.jasonObj.question[i].type}"
        >${this.jasonObj.question[i].option}</span
      >
    </button>`
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
              <button class="ev-button ${this.jasonObj.divId}">
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
      <div class="ev-component ev-dragAndDrop-component ev-component-full">
        <div class="ev-component-inner ev-dragAndDrop-inner">
          <div
            class="ev-display-title ev-dragAndDrop-title ev-component-title"
          >
            <h4 class="ev-component-title-inner">${title}</h4>
          </div>
          <div class="ev-body ev-component-instruction">
            <div class="ev-component-instruction-inner">
              ${statement}
            </div>
          </div>
          <div
            role="application"
            class="ev-dragAndDrop-widget ev-component-widget ev-clearfix ev-drop-zones-bottom"
          >
            <div id="${this.jasonObj.divId}-parentDiv" class="ev-drag-and-drop-layout-container droptarget">          
              ${choices}
            </div>
              <div class="ev-drop-zones ev-clearfix">
                <div class="ev-drop-zone">
                  <div class="ev-body ev-drop-zone-text point-b">
                    <p>True</p>
                  </div>
                  <div class="ev-drop-zone-content">
                    <div id="${this.jasonObj.divId}-true" class="ev-drop-zone-dropped-items droptarget"></div>
                  </div>
                </div>
                <div class="ev-drop-zone">
                  <div class="ev-body ev-drop-zone-text ev-body">
                    <p>False</p>
                  </div>
                  <div class="ev-drop-zone-content">
                    <div id="${this.jasonObj.divId}-false" class="ev-drop-zone-dropped-items droptarget"></div>
                  </div>
                </div>
              </div>
            
          </div>
          <div class="ev-buttons ev-buttons-full">
            <div class="ev-buttons-display" style="visibility: visible;">
              <div class="ev-buttons-display-inner">
                <span
                  aria-hidden="false"
                  style="opacity: 1; transform: none;"
                  >${remainingAttempts} </span
                ><span>${attemptsText}</span>
              </div>
            </div>
            <div class="ev-buttons-cluster-bottom ev-clearfix">
              <button
                id="${this.jasonObj.divId}-submit"
                class="ev-button ev-primary ev-buttons-action ${this.jasonObj.divId}"
                aria-hidden="true"
                aria-label="Select this button to submit your answer"
                disabled=""
              >
                <span class="ev-button-text">Submit</span></button
              ><button
                disabled=""
                class="ev-button ev-primary ev-buttons-feedback ev-button-icon-before ${this.jasonObj.divId}"
                aria-label="Select this button to show the feedback"
              >
                <span class="ev-button-text">Show Feedback</span>
              </button>
              <div class="ev-buttons-marking-icon ev-icon ev-icon-check" style="display: none;"></div>
              <div class="ev-buttons-marking-icon ev-icon ev-icon-cross" style="display: none;"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  </div>
<p id="demo"></p>`;

    this.initialize = () => {
        $(".truefalse-module#" + this.jasonObj.divId).append(module);

        /* Events fired on the drag target */
        $("#" + this.jasonObj.divId).on("dragstart", (event) => {
            event.originalEvent.dataTransfer.setData("Text", event.target.id);
        });

        $("#" + this.jasonObj.divId).on("dragend", (event) => { });

        $("#" + this.jasonObj.divId + "-true").on("dragenter", (event) => {
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });

        $("#" + this.jasonObj.divId + "-false").on("dragenter", (event) => {
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });

        $("#" + this.jasonObj.divId).on("dragover", (event) => {
            event.preventDefault();
        });

        $("#" + this.jasonObj.divId + "-false").on("dragover", (event) => {
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });

        $("#" + this.jasonObj.divId + "-true").on("dragover", (event) => {
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });

        $("#" + this.jasonObj.divId + "-true").on("dragleave", (event) => {
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .parent()
                .css("border", "2px solid #66cc99");
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .siblings()
                .css("background-color", "#66cc99");
        });
        $("#" + this.jasonObj.divId + "-false").on("dragleave", (event) => {
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .parent()
                .css("border", "2px solid #66cc99");
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .siblings()
                .css("background-color", "#66cc99");
        });

        $("#" + this.jasonObj.divId).on("drop", (event) => {
            event.preventDefault();
            if ($(event.target).hasClass("droptarget")) {
                let data = event.originalEvent.dataTransfer.getData("Text");
                event.target.prepend(document.getElementById(data));

                questionLength = $("#" + this.jasonObj.divId + "-parentDiv button")
                    .length;
                if (questionLength == 0) {
                    btnSubmit = $("#" + this.jasonObj.divId + "-submit");
                    btnSubmit.prop("disabled", false);
                }
                $("#" + this.jasonObj.divId + "-true")
                    .parent()
                    .parent()
                    .css("border", "2px solid #66cc99");
                $("#" + this.jasonObj.divId + "-true")
                    .parent()
                    .siblings()
                    .css("background-color", "#66cc99");
                $("#" + this.jasonObj.divId + "-false")
                    .parent()
                    .parent()
                    .css("border", "2px solid #66cc99");
                $("#" + this.jasonObj.divId + "-false")
                    .parent()
                    .siblings()
                    .css("background-color", "#66cc99");
            }
        });

        $("button." + this.jasonObj.divId + "-dragtarget").on("click", (e) => { });

        //show popup by clicking submit button

        $("button." + this.jasonObj.divId).on("click", (e) => {
            if ($.trim($(e.target).children().text()) === "Submit") {
                if (isTrueAnswerCorrect() || isFalseAnswerCorrect()) {
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
                    // enable feedback button
                    // enableFeedback(e);
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
                        // button change to Show Answer
                        enableShowAnswer(e);
                        // red cross icon show between feedback button
                        incorrectIcon(e);
                        //enable feedback button.
                        enableFeedback(e);
                    } else if (remainingAttempts == 1) {
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
                    } else {
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
                trueAnswerList = new Array();
                if ($("#" + this.jasonObj.divId + "-true button").length != 0) {
                    saveTrueList = $("#" + this.jasonObj.divId + "-true").html();
                    console.log("saveTrueList");
                    console.log(saveTrueList);
                    $("#" + this.jasonObj.divId + "-true button").each(function () {
                        trueAnswerList.push($(this).find("span").attr("data-ans-id"));
                    });
                }
                else {
                    trueAnswerList = new Array();
                    trueAnswerList.push(0);
                }
                if ($("#" + this.jasonObj.divId + "-false button").length != 0) {
                 
                    saveFalseList = $("#" + this.jasonObj.divId + "-false").html();
                    console.log("saveFalseList");
                    $("#" + this.jasonObj.divId + "-false button").each(function () {
                        falseAnswerList.push($(this).find("span").attr("data-ans-id"));
                    });
                    console.log(saveFalseList);
                }
                else {
                    falseAnswerList = new Array();
                }
            } else if ($.trim($(e.target).children().text()) == "Reset") {
                // change reset button to submit button
                btnSubmit.children().text("Submit");
                // disable submit button
                btnSubmit.prop("disabled", true);
                // disable feedback button
                disableFeedback(e);
                // enable All multiple choice
                enableChoice(e);
            } else if ($.trim($(e.target).children().text()) == "Close") {
              
                var questionId = this.jasonObj.divId.split('-')[1]
                if (autoTrigger == 1) {
                    autoTrigger = 0;
                }
                else {
                    var ModuleId = $("#hdnModuleId").val();
                    var CourseId = $("#hdnCourseId-" + questionId).val();
                    var ApplicatiionPath = $("#hdnApplicatonPath").val();
var freetext = "";
                    var url = ApplicatiionPath + '/Users/Courses/SubmitAnswer';
                    var requestPerameters = {
                        "id": ModuleId,
                        "course": CourseId,
                        "answer": trueAnswerList,
                        "question": questionId,
                        "flagged": false,
"FreeTextAnswer": freetext
                    }
                    AjaxPostRequestWithRequestPerameters(url, requestPerameters, function (response) {
                        if (response.Feedback) {
                            if (response.IsComplete) {
                                console.log("Alhumdulillah");
                                var nextModuleId = $("#hdnNextModuleId").val();
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
            } else if ($.trim($(e.target).children().text()) == "Show Feedback") {
                $("#ev-prompt-body-" + this.jasonObj.divId).empty();
                $("#ev-prompt-body-" + this.jasonObj.divId).append(jasonObj.feedback);
                $("#myModal-" + this.jasonObj.divId).modal("show");
            } else if ($.trim($(e.target).children().text()) == "Show Answer") {
                showCorrectAnswer();
            } else if ($.trim($(e.target).children().text()) == "Hide Answer") {
                hideCorrectAnswer();
            }
        });

        ////////////////////USMAN
        let spaceFound = 0;
        Array(length)
            .fill()
            .map((item, i) => {
                if (this.jasonObj.question[i].selected == "") {
                    spaceFound = 1;
                }
            })
            .join("");

        if (spaceFound == 0) {
            Array(length)
                .fill()
                .map((item, i) => {
                    if (this.jasonObj.question[i].selected == "true") {
                        //select Radio Button
                        $("#" + this.jasonObj.divId + "-true").append(
                            $("#" + this.jasonObj.divId + "-parentDiv")
                                .children()
                                .eq(0)
                        );
                    } else if (this.jasonObj.question[i].selected == "false") {
                        //select Radio Button
                        $("#" + this.jasonObj.divId + "-false").append(
                            $("#" + this.jasonObj.divId + "-parentDiv")
                                .children()
                                .eq(0)
                        );
                    }
                })
                .join("");
            autoTrigger = 1;
            console.log(this.jasonObj.divId);
            //"button." + this.jasonObj.divId
            btnSubmit = $("#" + this.jasonObj.divId + "-submit");
            btnSubmit.prop("disabled", false);
            $("button." + this.jasonObj.divId).trigger("click");
            $("#myModal-" + this.jasonObj.divId).modal("hide");
        }

        ////////////////////USMAN
    };

    let isTrueAnswerCorrect = () => {
        let trueList = new Array();
        let resultCounter = 0;
        $("#" + this.jasonObj.divId + "-true button").each((i, li) => {
            trueList.push($(li));
        });
        if (trueList.length == trueAnswer.length) {
            if (trueList.length !== 0) {
                for (let i = 0; i < trueList.length; i++) {
                    for (let j = 0; j < trueAnswer.length; j++) {
                        if ($.trim(trueList[i].children("span").text()) == trueAnswer[j])
                            resultCounter++;
                    }
                }
                return trueAnswer.length == resultCounter ? true : false;
            }
            return false;
        }
        return false;
    };

    let isFalseAnswerCorrect = () => {
        let falseList = new Array();
        let resultCounter = 0;
        $("#" + this.jasonObj.divId + "-false button").each((i, li) => {
            falseList.push($(li));
        });
        if (falseList.length == falseAnswer.length) {
            if (falseList.length !== 0) {
                for (let i = 0; i < falseList.length; i++) {
                    for (let j = 0; j < falseAnswer.length; j++) {
                        if ($.trim(falseList[i].children("span").text()) == falseAnswer[j])
                            resultCounter++;
                    }
                }
                return falseAnswer.length == resultCounter ? true : false;
            }
            return false;
        }
        return false;
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
            .text(" final attempt");
    };

    let enableReset = () => {
     btnSubmit.children().text("Reset");
    };

    let enableHideAnswer = () => {
        btnSubmit.children().text("Hide Answer");
    };

    let enableShowAnswer = () => {
        btnSubmit.children().text("Show Answer");
    };

    let hideCorrectAnswer = () => {
        if ($("#" + this.jasonObj.divId + "-true button").length != 0) {
            $("#" + this.jasonObj.divId + "-true button").remove();
            $("#" + this.jasonObj.divId + "-true").append(saveTrueList);
        }
        if ($("#" + this.jasonObj.divId + "-false button").length != 0) {
            $("#" + this.jasonObj.divId + "-false button").remove();
            $("#" + this.jasonObj.divId + "-false").append(saveFalseList);
        }
        enableShowAnswer();
    };

    let showCorrectAnswer = () => {
        let falseList = new Array();
        if ($("#" + this.jasonObj.divId + "-true button").length != 0) {
            $("#" + this.jasonObj.divId + "-true button").each((i, li) => {
                if (
                    $("#" + this.jasonObj.divId + "-true button:eq(" + i + ")")
                        .children("i")
                        .hasClass("ev-icon-cross")
                ) {
                    $(li).children("i").removeClass("ev-icon-cross");
                    $(li).children("i").addClass("ev-icon-check");
                    falseList.push($(li));
                }
            });
            $("#" + this.jasonObj.divId + "-false").append(falseList);
        }

        let trueList = new Array();
        if ($("#" + this.jasonObj.divId + "-false button").length != 0) {
            $("#" + this.jasonObj.divId + "-false button").each((i, li) => {
                if (
                    $("#" + this.jasonObj.divId + "-false button:eq(" + i + ")")
                        .children("i")
                        .hasClass("ev-icon-cross")
                ) {
                    $(li).children("i").removeClass("ev-icon-cross");
                    $(li).children("i").addClass("ev-icon-check");
                    trueList.push($(li));
                }
            });
            $("#" + this.jasonObj.divId + "-true").append(trueList);
        }

        enableHideAnswer();
    };

    let incorrectAnswer = () => {
        let trueList = new Array();
        $("#" + this.jasonObj.divId + "-true button").each((i, li) => {
            trueList.push($(li));
        });
        for (let i = 0; i < trueList.length; i++) {
            for (let j = 0; j < trueAnswer.length; j++) {
                if ($.trim(trueList[i].children("span").text()) == trueAnswer[j]) {
                    trueList[i].children("i").removeClass("ev-icon-move");
                    trueList[i].children("i").removeClass("ev-icon-cross");
                    trueList[i].children("i").addClass("ev-icon-check");
                    break;
                } else {
                    trueList[i].children("i").removeClass("ev-icon-move");
                    trueList[i].children("i").addClass("ev-icon-cross");
                }
            }
        }

        let falseList = new Array();
        $("#" + this.jasonObj.divId + "-false button").each((i, li) => {
            falseList.push($(li));
        });
        for (let i = 0; i < falseList.length; i++) {
            for (let j = 0; j < falseAnswer.length; j++) {
                if ($.trim(falseList[i].children("span").text()) == falseAnswer[j]) {
                    falseList[i].children("i").removeClass("ev-icon-move");
                    falseList[i].children("i").removeClass("ev-icon-cross");
                    falseList[i].children("i").addClass("ev-icon-check");
                    break;
                } else {
                    falseList[i].children("i").removeClass("ev-icon-move");
                    falseList[i].children("i").addClass("ev-icon-cross");
                }
            }
        }
        disableChoice();
    };

    let correctAnswer = (e) => {
        let setTrueClass = $("#" + this.jasonObj.divId + "-true button").children(
            "i"
        );
        let setFalseClass = $("#" + this.jasonObj.divId + "-false button").children(
            "i"
        );
        setTrueClass.removeClass("ev-icon-move");
        setTrueClass.addClass("ev-icon-check");

        setFalseClass.removeClass("ev-icon-move");
        setFalseClass.addClass("ev-icon-check");
        disableChoice();
    };

    let disableChoice = () => {
        // siblingsOfLabel.addClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-true").addClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-false").addClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-true button").attr("draggable", "false");
        $("#" + this.jasonObj.divId + "-false button").attr("draggable", "false");
        $("#" + this.jasonObj.divId + "-parentDiv").attr("draggable", "false");
        $("#" + this.jasonObj.divId + "-true button").css("cursor", "default");
        $("#" + this.jasonObj.divId + "-false button").css("cursor", "default");
        // $("." + this.jasonObj.divId + "-li").off("click");
    };

    let enableChoice = (e) => {
        $("#" + this.jasonObj.divId + "-true").removeClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-false").removeClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-true button").attr("draggable", "true");
        $("#" + this.jasonObj.divId + "-false button").attr("draggable", "true");
        $("#" + this.jasonObj.divId + "-parentDiv").attr("draggable", "true");
        $("#" + this.jasonObj.divId + "-true button").css("cursor", "move");
        $("#" + this.jasonObj.divId + "-false button").css("cursor", "move");
        $("#" + this.jasonObj.divId + "-true button").remove();
        $("#" + this.jasonObj.divId + "-false button").remove();
        $("#" + this.jasonObj.divId + "-parentDiv").append(choices);
        // $("." + this.jasonObj.divId + "-li").on("click", (e) => {
        //   setTimeout(() => {
        //     questionLength = $("#" + this.jasonObj.divId + "-question button")
        //       .length;
        //     if (questionLength == 0) {
        //       btnSubmit = $("#" + this.jasonObj.divId + "-submit");
        //       btnSubmit.prop("disabled", false);
        //     }
        //   }, 600);
        // });
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

    let incorrectIcon = (e) => {
        $(e.target).siblings("div.ev-icon-cross").css("display", "block");
    };

    let correctIcon = (e) => {
        $(e.target).siblings("div.ev-icon-check").css("display", "block");
    };
}

//This for Multiple Choices

function TrueFalse2(jasonObj) {
    this.jasonObj = jasonObj;
    var autoTrigger = 0;
    let remainingAttempts = this.jasonObj.attempts;
    let title = this.jasonObj.title;
    let length = this.jasonObj.question.length;
    let trueAnswerList = new Array();
    let anslength = this.jasonObj.answer.length;
    let statement = this.jasonObj.statement;
    let trueAnswer = new Array();
    let falseAnswer = new Array();
    let questionID = this.jasonObj.divId.split('-')[1];
    let saveTrueList;
    let saveFalseList;
    let btnSubmit;
    let popUpMessage1 = "Incorrect, this is a incorrect answer"; //"That is incorrect. Re-read the module then try again.";
    let popUpMessage2 = "Incorrect, this is a incorrect answer";//"That is incorrect and your final attempt";
    let popUpMessage3 = "Well done, this is a correct answer.";
    let attemptsText =
        remainingAttempts == 1 ? " final attempt" : " attempts remaining";
    Array(length)
        .fill()
        .map((item, i) => {
            if (this.jasonObj.question[i].type === "true") {
                trueAnswer.push(this.jasonObj.question[i].option);
            } else {
                falseAnswer.push(this.jasonObj.question[i].option);
            }
        })
        .join("");
    let questionLength;
    let choices = `${Array(anslength)
        .fill()
        .map(
            (item, i) => `
      <button
      draggable="true"
      id="${this.jasonObj.divId}-dragtarget${[i]}"
      class="ev-draggable-item ev-component-item ${this.jasonObj.divId
                }-dragtarget"
      style="left: 0px; top: 0px;"
    >
      <i
        class="ev-icon ev-draggable-item-icon ev-icon-move"
      ></i
      ><span id="${this.jasonObj.question[i].type}"
        >${this.jasonObj.answer[i].title}</span
      >
    </button>`
        )
        .join("")}`;

    let opitions = `${Array(length).fill()
        .map((item, i) => `
 
                <div  class="ev-drop-zone">
                  <div class="ev-body ev-drop-zone-text point-b">
                    <p>${this.jasonObj.question[i].option}</p>
                  </div>
                  <div class="ev-drop-zone-content">
                    <div id="${this.jasonObj.divId + i}" style="min-height: 50px !important ;" class="ev-drop-zone-dropped-items droptarget"></div>
                  </div>
                </div>`
        ).join("")
        }`;


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
                    <i class="ev-icon-cross-circle ev-icon" role="img"  aria-label="incorrect"></i>
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
              <button class="ev-button ${this.jasonObj.divId}">
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
      <div class="ev-component ev-dragAndDrop-component ev-component-full">
        <div class="ev-component-inner ev-dragAndDrop-inner">
          <div
            class="ev-display-title ev-dragAndDrop-title ev-component-title"
          >
            <h4 class="ev-component-title-inner">${title}</h4>
          </div>
          <div class="ev-body ev-component-instruction">
            <div class="ev-component-instruction-inner">
              ${statement}
            </div>
          </div>
          <div
            role="application"
            class="ev-dragAndDrop-widget ev-component-widget ev-clearfix ev-drop-zones-bottom"
          >
            <div id="${this.jasonObj.divId}-parentDiv" class="ev-drag-and-drop-layout-container droptarget">          
              ${choices}
            </div>
             <div role="application" class="ev-dragAndDrop-widget ev-component-widget ev-clearfix ev-drop-zones-bottom ev-drop-zones-compact ev-drop-zones-horizontal-layout">
                                                        <div class="ev-drag-and-drop-layout-container" aria-hidden="true">
                                                            <div class="ev-draggable-items"></div><div class="ev-drop-zones ev-clearfix">
                                                           ${opitions}
                                                              
                                                              
                                                                
                                                            </div>
                                                        </div>

                                                    </div>

     <!--   <div class="ev-drop-zones ev-clearfix">

                ${opitions}
               
            
            
          </div> -->
          <div class="ev-buttons ev-buttons-full">
            <div class="ev-buttons-display" style="visibility: visible;">
              <div class="ev-buttons-display-inner">
                <span
                  aria-hidden="false"
                  style="opacity: 1; transform: none;"
                  >${remainingAttempts} </span
                ><span>${attemptsText}</span>
              </div>
            </div>
            <div class="ev-buttons-cluster-bottom ev-clearfix">
              <button
                id="${this.jasonObj.divId}-submit"
                class="ev-button ev-primary ev-buttons-action ${this.jasonObj.divId}"
                aria-hidden="true"
                aria-label="Select this button to submit your answer"
                disabled=""
              >
                <span class="ev-button-text">Submit</span></button
              ><button
                disabled=""
                class="ev-button ev-primary ev-buttons-feedback ev-button-icon-before ${this.jasonObj.divId}"
                aria-label="Select this button to show the feedback"
              >
                <span class="ev-button-text">Show Feedback</span>
              </button>
              <div class="ev-buttons-marking-icon ev-icon ev-icon-check" style="display: none;"></div>
              <div class="ev-buttons-marking-icon ev-icon ev-icon-cross" style="display: none;"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  </div>
<p id="demo"></p>`;

    this.initialize = () => {
        $(".truefalse-module#" + this.jasonObj.divId).append(module);

        /* Events fired on the drag target */
        $("#" + this.jasonObj.divId).on("dragstart", (event) => {
            event.originalEvent.dataTransfer.setData("Text", event.target.id);
        });

        $("#" + this.jasonObj.divId).on("dragend", (event) => { });

        $("#" + this.jasonObj.divId + "-true").on("dragenter", (event) => {
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });

        $("#" + this.jasonObj.divId + "-false").on("dragenter", (event) => {
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });

        $("#" + this.jasonObj.divId).on("dragover", (event) => {
            event.preventDefault();
        });

        $("#" + this.jasonObj.divId + "-false").on("dragover", (event) => {
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });

        $("#" + this.jasonObj.divId + "0").on("dragover", (event) => {

            $("#" + this.jasonObj.divId + "0")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "0")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });
        $("#" + this.jasonObj.divId + "1").on("dragover", (event) => {

            $("#" + this.jasonObj.divId + "1")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "1")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });
        $("#" + this.jasonObj.divId + "2").on("dragover", (event) => {

            $("#" + this.jasonObj.divId + "2")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "2")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });



        $("#" + this.jasonObj.divId + "0").on("dragleave", (event) => {
            $("#" + this.jasonObj.divId + "0")
                .parent()
                .parent()
                .css("border", "2px solid #66cc99");
            $("#" + this.jasonObj.divId + "0")
                .parent()
                .siblings()
                .css("background-color", "#66cc99");
        });
        $("#" + this.jasonObj.divId + "1").on("dragleave", (event) => {
            $("#" + this.jasonObj.divId + "1")
                .parent()
                .parent()
                .css("border", "2px solid #66cc99");
            $("#" + this.jasonObj.divId + "1")
                .parent()
                .siblings()
                .css("background-color", "#66cc99");
        });

        $("#" + this.jasonObj.divId + "2").on("dragleave", (event) => {
            $("#" + this.jasonObj.divId + "2")
                .parent()
                .parent()
                .css("border", "2px solid #66cc99");
            $("#" + this.jasonObj.divId + "2")
                .parent()
                .siblings()
                .css("background-color", "#66cc99");
        });
        //   $("#" + this.jasonObj.divId + "-false").on("dragleave", (event) => {
        //       $("#" + this.jasonObj.divId + "-false")
        //           .parent()
        //           .parent()
        //           .css("border", "2px solid #66cc99");
        //       $("#" + this.jasonObj.divId + "-false")
        //           .parent()
        //           .siblings()
        //           .css("background-color", "#66cc99");
        //   });

        $("#" + this.jasonObj.divId).on("drop", (event) => {
            event.preventDefault();
            if ($(event.target).hasClass("droptarget")) {
                let data = event.originalEvent.dataTransfer.getData("Text");
                event.target.prepend(document.getElementById(data));

                questionLength = $("#" + this.jasonObj.divId + "-parentDiv button")
                    .length;
                if (questionLength == 0) {
                    btnSubmit = $("#" + this.jasonObj.divId + "-submit");
                    btnSubmit.prop("disabled", false);
                }
                $("#" + this.jasonObj.divId + "-true")
                    .parent()
                    .parent()
                    .css("border", "2px solid #66cc99");
                $("#" + this.jasonObj.divId + "-true")
                    .parent()
                    .siblings()
                    .css("background-color", "#66cc99");
                $("#" + this.jasonObj.divId + "-false")
                    .parent()
                    .parent()
                    .css("border", "2px solid #66cc99");
                $("#" + this.jasonObj.divId + "-false")
                    .parent()
                    .siblings()
                    .css("background-color", "#66cc99");
            }
        });

        $("button." + this.jasonObj.divId + "-dragtarget").on("click", (e) => { });

        //show popup by clicking submit button

        $("button." + this.jasonObj.divId).on("click", (e) => {
            if ($.trim($(e.target).children().text()) === "Submit") {
              //  debugger
                var ans = "s";
                var ques = "q";
                for (var i = 0; i < length; i++) {
                    $("#" + this.jasonObj.divId + "-dragtarget" + i).css("cursor", "default");
                    $("#" + this.jasonObj.divId + "-dragtarget" + i).prop("draggable", false);
                    if ($("#" + this.jasonObj.divId + i).find("button").length == 1) {
                        ans = this.jasonObj.answer[0].type;
                        ques = this.jasonObj.question[i].type;
                        trueAnswerList = new Array();
                        trueAnswerList.push(this.jasonObj.question[i].answerid);

                        $("#" + this.jasonObj.divId + "-dragtarget" + i).find("i").removeClass("ev-icon-move");
                        $("#" + this.jasonObj.divId + "-dragtarget" + i).find("i").addClass("ev-icon-check");
                    }
                }
                //saving data in DB
               
                if (ans == ques) {
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
                        // .addClass("ev-icon-checkmark-circle");
                        .addClass("ev-icon-checkmark-circle");
                    $("#ev-prompt-title-" + this.jasonObj.divId)
                        .siblings("div.ev-notify-glyph")
                        .children()
                        .children()
                        // .addClass("ev-icon-checkmark-circle");
                        .css("color", "green");
                    $("#myModal-" + this.jasonObj.divId).modal("show");
                    // enable feedback button
                   // enableFeedback(e);
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
                        // button change to Show Answer
                        enableShowAnswer(e);
                        // red cross icon show between feedback button
                        incorrectIcon(e);
                        //enable feedback button.
                        enableFeedback(e);
                    } else if (remainingAttempts == 1) {
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

                    } else {

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
                if ($("#" + this.jasonObj.divId + "-true button").length != 0) {
                    saveTrueList = $("#" + this.jasonObj.divId + "-true").html();
                }
                if ($("#" + this.jasonObj.divId + "-false button").length != 0) {
                    saveFalseList = $("#" + this.jasonObj.divId + "-false").html();
                }
            } else if ($.trim($(e.target).children().text()) == "Reset") {
                // change reset button to submit button
                btnSubmit.children().text("Submit");
                // disable submit button
                btnSubmit.prop("disabled", true);
                //Reseting options
                for (var i = 0; i < length; i++) {
                    if ($("#" + this.jasonObj.divId + i).find("button").length == 1) {
                        $("#" + this.jasonObj.divId + i).empty();
                    }
                }
                // disable feedback button
                disableFeedback(e);
                // enable All multiple choice
                enableChoice(e);
            } else if ($.trim($(e.target).children().text()) == "Close") {
                PushDataToDB();
                trueAnswerList = new Array();
                $("#myModal-" + this.jasonObj.divId).modal("hide");
            } else if ($.trim($(e.target).children().text()) == "Show Feedback") {
                $("#ev-prompt-body-" + this.jasonObj.divId).empty();
                $("#ev-prompt-body-" + this.jasonObj.divId).append(jasonObj.feedback);
                $("#myModal-" + this.jasonObj.divId).modal("show");
            } else if ($.trim($(e.target).children().text()) == "Show Answer") {
                showCorrectAnswer();
            } else if ($.trim($(e.target).children().text()) == "Hide Answer") {
                hideCorrectAnswer();
            }
        });

        ////////////////////USMAN
        let spaceFound = false;
        if (autoTrigger == 0) {
            Array(length)
                .fill()
                .map((item, i) => {
                    if (this.jasonObj.question[i].selected == "" || this.jasonObj.question[i].selected == "false") {
                        console.log("ss " + i++);                    
                    } else {
                        spaceFound = true;
                    }
                })
                .join("");

        }
        if (spaceFound == true) {
            Array(length)
                .fill()
                .map((item, i) => {
                    if (this.jasonObj.question[i].selected == "true") {
                        //select Radio Button
                        $("#" + this.jasonObj.divId + i).append(
                            $("#" + this.jasonObj.divId + "-parentDiv")
                                .children()
                                .eq(0)
                        );
                    }// else if (this.jasonObj.question[i].selected == "false") {
                    //    //select Radio Button
                    //    $("#" + this.jasonObj.divId + i).append(
                    //        $("#" + this.jasonObj.divId + "-parentDiv")
                    //            .children()
                    //            .eq(0)
                    //    );
                    //}
                })
                .join("");
            autoTrigger = 1;
            console.log('auto trigger');
            btnSubmit = $("#" + this.jasonObj.divId + "-submit");
            btnSubmit.prop("disabled", false);
            $("button." + this.jasonObj.divId).trigger("click");
            $("#myModal-" + this.jasonObj.divId).modal("hide");
        }

        ////////////////////USMAN
    };

    let isTrueAnswerCorrect = () => {
        let trueList = new Array();
        let resultCounter = 0;
        $("#" + this.jasonObj.divId + "-true button").each((i, li) => {
            trueList.push($(li));
        });
        if (trueList.length == trueAnswer.length) {
            if (trueList.length !== 0) {
                for (let i = 0; i < trueList.length; i++) {
                    for (let j = 0; j < trueAnswer.length; j++) {
                        if ($.trim(trueList[i].children("span").text()) == trueAnswer[j])
                            resultCounter++;
                    }
                }
                return trueAnswer.length == resultCounter ? true : false;
            }
            return false;
        }
        return false;
    };

    let isFalseAnswerCorrect = () => {
        let falseList = new Array();
        let resultCounter = 0;
        $("#" + this.jasonObj.divId + "-false button").each((i, li) => {
            falseList.push($(li));
        });
        if (falseList.length == falseAnswer.length) {
            if (falseList.length !== 0) {
                for (let i = 0; i < falseList.length; i++) {
                    for (let j = 0; j < falseAnswer.length; j++) {
                        if ($.trim(falseList[i].children("span").text()) == falseAnswer[j])
                            resultCounter++;
                    }
                }
                return falseAnswer.length == resultCounter ? true : false;
            }
            return false;
        }
        return false;
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
            .text(" final attempt");
    };

    let enableReset = () => {
        btnSubmit.children().text("Reset");
    };

    let enableHideAnswer = () => {
        btnSubmit.children().text("Hide Answer");
    };

    let enableShowAnswer = () => {
        btnSubmit.children().text("Show Answer");
    };

    let hideCorrectAnswer = () => {
        var ans = '<button draggable="false" id="module-' + questionID + '-dragtarget0" class="ev-draggable-item ev-component-item module-' + questionID + '-dragtarget" style="left: 0px; top: 0px; cursor: default;">';
        ans += '<i class="ev-icon ev-draggable-item-icon ev-icon-move" ></i> <span id="true">' + this.jasonObj.answer[0].title + '</span> </button>';
        $(rightAnswerid).empty();
        $(wrongAnswerid).empty();
        $(wrongAnswerid).append(ans);
        enableShowAnswer();
    };
    let wrongAnswerid;
    let rightAnswerid;
    let showCorrectAnswer = () => {


        for (var i = 0; i < length; i++) {
            if ($("#" + this.jasonObj.divId + i).find("button").length == 1) {
                wrongAnswerid = "#" + this.jasonObj.divId + i;
                $(wrongAnswerid).empty();


            }
            if (this.jasonObj.answer[0].type == this.jasonObj.question[i].type) {
                var ans = '<button draggable="false" id="module-' + questionID + '-dragtarget0" class="ev-draggable-item ev-component-item module-' + questionID + '-dragtarget" style="left: 0px; top: 0px; cursor: default;">';
                ans += '<i class="ev-icon ev-draggable-item-icon ev-icon-check" ></i> <span id="true">' + this.jasonObj.answer[0].title + '</span> </button>';
                rightAnswerid = "#" + this.jasonObj.divId + i;
                $(rightAnswerid).empty();
                $(rightAnswerid).append(ans);
            }
        }

        enableHideAnswer();
    };

    let incorrectAnswer = () => {
        let trueList = new Array();
        $("#" + this.jasonObj.divId + "-true button").each((i, li) => {
            trueList.push($(li));
        });
        for (let i = 0; i < trueList.length; i++) {
            for (let j = 0; j < trueAnswer.length; j++) {
                if ($.trim(trueList[i].children("span").text()) == trueAnswer[j]) {
                    trueList[i].children("i").removeClass("ev-icon-move");
                    trueList[i].children("i").removeClass("ev-icon-cross");
                    trueList[i].children("i").addClass("ev-icon-check");
                    break;
                } else {
                    trueList[i].children("i").removeClass("ev-icon-move");
                    trueList[i].children("i").addClass("ev-icon-cross");
                }
            }
        }

        let falseList = new Array();
        $("#" + this.jasonObj.divId + "-false button").each((i, li) => {
            falseList.push($(li));
        });
        for (let i = 0; i < falseList.length; i++) {
            for (let j = 0; j < falseAnswer.length; j++) {
                if ($.trim(falseList[i].children("span").text()) == falseAnswer[j]) {
                    falseList[i].children("i").removeClass("ev-icon-move");
                    falseList[i].children("i").removeClass("ev-icon-cross");
                    falseList[i].children("i").addClass("ev-icon-check");
                    break;
                } else {
                    falseList[i].children("i").removeClass("ev-icon-move");
                    falseList[i].children("i").addClass("ev-icon-cross");
                }
            }
        }
        disableChoice();
    };

    let correctAnswer = (e) => {
        let setTrueClass = $("#" + this.jasonObj.divId + "-true button").children(
            "i"
        );
        let setFalseClass = $("#" + this.jasonObj.divId + "-false button").children(
            "i"
        );
        setTrueClass.removeClass("ev-icon-move");
        setTrueClass.addClass("ev-icon-check");

        setFalseClass.removeClass("ev-icon-move");
        setFalseClass.addClass("ev-icon-check");
        disableChoice();
    };

    let disableChoice = () => {
        // siblingsOfLabel.addClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-true").addClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-false").addClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-true button").attr("draggable", "false");
        $("#" + this.jasonObj.divId + "-false button").attr("draggable", "false");
        $("#" + this.jasonObj.divId + "-parentDiv").attr("draggable", "false");
        $("#" + this.jasonObj.divId + "-true button").css("cursor", "default");
        $("#" + this.jasonObj.divId + "-false button").css("cursor", "default");
        // $("." + this.jasonObj.divId + "-li").off("click");
    };

    let enableChoice = (e) => {
        for (var i = 0; i < length; i++) {
            $("#" + this.jasonObj.divId + i)
                .parent()
                .parent()
                .css("border", "2px solid #66cc99");
            $("#" + this.jasonObj.divId + i)
                .parent()
                .siblings()
                .css("background-color", "#66cc99");
        }
        $("#" + this.jasonObj.divId + "-true").removeClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-false").removeClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-true button").attr("draggable", "true");
        $("#" + this.jasonObj.divId + "-false button").attr("draggable", "true");
        $("#" + this.jasonObj.divId + "-parentDiv").attr("draggable", "true");
        $("#" + this.jasonObj.divId + "-true button").css("cursor", "move");
        $("#" + this.jasonObj.divId + "-false button").css("cursor", "move");
        $("#" + this.jasonObj.divId + "-true button").remove();
        $("#" + this.jasonObj.divId + "-false button").remove();
        $("#" + this.jasonObj.divId + "-parentDiv").append(choices);
        // $("." + this.jasonObj.divId + "-li").on("click", (e) => {
        //   setTimeout(() => {
        //     questionLength = $("#" + this.jasonObj.divId + "-question button")
        //       .length;
        //     if (questionLength == 0) {
        //       btnSubmit = $("#" + this.jasonObj.divId + "-submit");
        //       btnSubmit.prop("disabled", false);
        //     }
        //   }, 600);
        // });
    };

    let hideAttempts = (e) => {
        $(e.target).parent().siblings().css("visibility", "hidden");
    };

    let disableSubmit = () => {
        btnSubmit.prop("disabled", true);
    };
    let PushDataToDB = () => {
       // debugger;
        //if (remainingAttempts <= 0) {
        //    trueAnswerList = new Array();
        //    $("#myModal-" + this.jasonObj.divId).modal("hide");
        //    return false;
        //}
        var questionId = this.jasonObj.divId.split('-')[1]
        if (autoTrigger == 1) {
            autoTrigger = 0;
        }
        else {
            var ModuleId = $("#hdnModuleId").val();
            var CourseId = $("#hdnCourseId-" + questionId).val();
            var ApplicatiionPath = $("#hdnApplicatonPath").val();
		var freetext = "";
            if (ApplicatiionPath == "") {
                hr = window.location.href.split("/");
                ApplicatiionPath = hr[0] + "//" + hr[2];
            }
            var url = ApplicatiionPath + '/Users/Courses/SubmitAnswer';
            var requestPerameters = {
                "id": ModuleId,
                "course": CourseId,
                "answer": trueAnswerList,
                "question": questionId,
                "flagged": false,
"FreeTextAnswer": freetext
            }
            AjaxPostRequestWithRequestPerameters(url, requestPerameters, function (response) {
                if (response.Feedback) {
                    if (response.IsComplete) {
                        console.log("Alhumdulillah");
                        var nextModuleId = $("#hdnNextModuleId").val();
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

        }
    }

    let enableFeedback = (e) => {
        $(e.target).siblings("button.ev-buttons-feedback").prop("disabled", false);
    };

    let disableFeedback = (e) => {
        $(e.target).siblings("button.ev-buttons-feedback").prop("disabled", true);
    };

    let incorrectIcon = (e) => {
        $(e.target).siblings("div.ev-icon-cross").css("display", "block");
    };

    let correctIcon = (e) => {
        $(e.target).siblings("div.ev-icon-check").css("display", "block");
    };
}

//This for True False With DropDown


function TrueFalse1(jasonObj) {
   // debugger
    this.jasonObj = jasonObj;
    let remainingAttempts = this.jasonObj.attempts;
    let title = this.jasonObj.title;
    var autoTrigger = 0;
    let questionID = this.jasonObj.divId.split('-')[1];
    let length = this.jasonObj.question.length;
    let statement = this.jasonObj.statement;
    let trueAnswerList = new Array();
    let trueAnswer = new Array();
    let falseAnswer = new Array();
    let Answer = new Array();
    let falAnswer = new Array();
    let saveTrueList;
    let saveFalseList;
    let btnSubmit;
    let popUpMessage1 = "Incorrect, this is a incorrect answer"; //"That is incorrect. Re-read the module then try again.";
    let popUpMessage2 = "Incorrect, this is a incorrect answer";//"That is incorrect and your final attempt";
    let popUpMessage3 = "Well done, this is a correct answer.";
    let popUpMessage4 = "";
    let attemptsText =
        remainingAttempts == 1 ? " final attempt" : " attempts remaining";
    Array(length)
        .fill()
        .map((item, i) => {
            if (this.jasonObj.question[i].type === "true") {
                trueAnswer.push(this.jasonObj.question[i].option);
            } else {
                falseAnswer.push(this.jasonObj.question[i].option);
            }
        })
        .join("");
    let questionLength;
    
    let choices = `${Array(length)
        .fill()
        .map(
            (item, i) => `
    <div><button
      draggable="false"
      id="${this.jasonObj.divId}-dragtarget${[i]}"
      class="ev-draggable-item ev-component-item ${this.jasonObj.divId
                }-dragtarget"
      style="left: 0px; top: 0px; width:100%;cursor: auto !important;"
    >
      
      <span id="${this.jasonObj.question[i].type}"
        >${this.jasonObj.question[i].option}</span
      >
    </button> </div>`
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
              <button class="ev-button ${this.jasonObj.divId}">
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
      <div class="ev-component ev-dragAndDrop-component ev-component-full">
        <div class="ev-component-inner ev-dragAndDrop-inner">
          <div
            class="ev-display-title ev-dragAndDrop-title ev-component-title"
          >
            <h4 class="ev-component-title-inner">${title}</h4>
          </div>
          <div class="ev-body ev-component-instruction">
            <div class="ev-component-instruction-inner">
              ${statement}
            </div>
          </div>
          <div
            role="application"
            class="ev-dragAndDrop-widget ev-component-widget ev-clearfix ev-drop-zones-bottom"
          >
           <style>
             input, button, select, textarea {                          
                          color: #454545;
                          text-align: left;
                          font-family: Signika;
                          font-size: 15px;
                          font-weight: 600;
                          line-height: 1.25;
                          }
           </style>
            <div id="${this.jasonObj.divId}-parentDiv" class="ev-drag-and-drop-layout-container droptarget">          
              ${choices}
            </div>
              <div class="ev-drop-zones ev-clearfix">
               
          
          </div>
          <div class="ev-buttons ev-buttons-full">
            <div class="ev-buttons-display" style="visibility: visible;">
              <div class="ev-buttons-display-inner">
                <span
                  aria-hidden="false"
                  style="opacity: 1; transform: none;"
                  >${remainingAttempts} </span
                ><span>${attemptsText}</span>
              </div>
            </div>
            <div class="ev-buttons-cluster-bottom ev-clearfix">
              <button
                id="${this.jasonObj.divId}-submit"
                class="ev-button ev-primary ev-buttons-action ${this.jasonObj.divId}"
                aria-hidden="true"
                aria-label="Select this button to submit your answer"
                
              disabled="">
                <span class="ev-button-text">Submit</span></button
              ><button
                
                class="ev-button ev-primary ev-buttons-feedback ev-button-icon-before ${this.jasonObj.divId}"
                aria-label="Select this button to show the feedback"
              disabled="">
                <span class="ev-button-text">Show Feedback</span>
              </button>
              <div class="ev-buttons-marking-icon ev-icon ev-icon-check" style="display: none;"></div>
              <div class="ev-buttons-marking-icon ev-icon ev-icon-cross" style="display: none;"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  </div>
<p id="demo"></p>`;

    this.initialize = () => {
        $(".truefalse-module#" + this.jasonObj.divId).append(module);
        /* Events fired on the drag target */
        $("#" + this.jasonObj.divId).on("dragstart", (event) => {
            event.originalEvent.dataTransfer.setData("Text", event.target.id);
        });

        $("#" + this.jasonObj.divId).on("dragend", (event) => { });

        $("#" + this.jasonObj.divId + "-true").on("dragenter", (event) => {
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });

        $("#" + this.jasonObj.divId + "-false").on("dragenter", (event) => {
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });

        $("#" + this.jasonObj.divId).on("dragover", (event) => {
            event.preventDefault();
        });

        $("#" + this.jasonObj.divId + "-false").on("dragover", (event) => {
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });

        $("#" + this.jasonObj.divId + "-true").on("dragover", (event) => {
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .parent()
                .css("border", "2px solid #339966");
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .siblings()
                .css("background-color", "#339966");
        });

        $("#" + this.jasonObj.divId + "-true").on("dragleave", (event) => {
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .parent()
                .css("border", "2px solid #66cc99");
            $("#" + this.jasonObj.divId + "-true")
                .parent()
                .siblings()
                .css("background-color", "#66cc99");
        });
        $("#" + this.jasonObj.divId + "-false").on("dragleave", (event) => {
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .parent()
                .css("border", "2px solid #66cc99");
            $("#" + this.jasonObj.divId + "-false")
                .parent()
                .siblings()
                .css("background-color", "#66cc99");
        });

        $(".ev-input-text").on("change", (event) => {
            var j = 0;
            for (var i = 1; i <= length; i++) {
                if ($("#inid" + i + "-" + questionID).val() != '') {
                    j++;
                }
            }
            if (j == length) {
                btnSubmit = $("#" + this.jasonObj.divId + "-submit");
                btnSubmit.prop("disabled", false);
            } else {
                btnSubmit = $("#" + this.jasonObj.divId + "-submit");
                btnSubmit.prop("disabled", true);
            }
            //  event.preventDefault();

        });


        $("button." + this.jasonObj.divId + "-dragtarget").on("click", (e) => { });

        //show popup by clicking submit button

        $("button." + this.jasonObj.divId).on("click", (e) => {

            if ($.trim($(e.target).children().text()) === "Submit") {
                var s = 0;
                trueAnswerList = new Array();
                for (var i = 1; i <= length; i++) {
                    if ($("#inid" + i + "-" + questionID).val().toLowerCase() == this.jasonObj.question[i - 1].type.toLowerCase()) {
                        s++;
                        // trueAnswerList.push(this.jasonObj.question[i - 1].answerid);
                    }
                    if ($("#inid" + i + "-" + questionID).val().toLowerCase() == "true") {

                        trueAnswerList.push(this.jasonObj.question[i - 1].answerid);
                      } 
                  //  trueAnswerList.push(this.jasonObj.question[i - 1].answerid);
                }
               

                if (trueAnswerList.length == 0) {
                    trueAnswerList.push(0);
                }
                
                PushDataToDB();
                if (s == length) {
                    debugger;
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
                        // .addClass("ev-icon-checkmark-circle");
                        .addClass("ev-icon-checkmark-circle");
                    $("#ev-prompt-title-" + this.jasonObj.divId)
                        .siblings("div.ev-notify-glyph")
                        .children()
                        .children()
                        // .addClass("ev-icon-checkmark-circle");
                        .css("color", "green");
                    $("#myModal-" + this.jasonObj.divId).modal("show");
                    //disable input

                    disableinput();
                    // enable feedback button
                  //  enableFeedback(e);
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
                        // button change to Show Answer
                        enableShowAnswer(e);
                        // red cross icon show between feedback button
                        incorrectIcon(e);
                        //enable feedback button.
                        enableFeedback(e);
                    } else if (remainingAttempts == 1) {
                        // main page change text remaining attempts to final attempts
                        if (s > 0) {
                            $("#ev-prompt-body-" + this.jasonObj.divId).text(popUpMessage2);
                        } else {
                            $("#ev-prompt-body-" + this.jasonObj.divId).text(popUpMessage1);
                        }
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
                    } else {
                        if (s > 0) {
                            $("#ev-prompt-body-" + this.jasonObj.divId).text(popUpMessage2);
                        } else {
                            $("#ev-prompt-body-" + this.jasonObj.divId).text(popUpMessage1);
                        }
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
                if ($("#" + this.jasonObj.divId + "-true button").length != 0) {
                    saveTrueList = $("#" + this.jasonObj.divId + "-true").html();
                }
                if ($("#" + this.jasonObj.divId + "-false button").length != 0) {
                    saveFalseList = $("#" + this.jasonObj.divId + "-false").html();
                }
            } else if ($.trim($(e.target).children().text()) == "Reset") {
                // change reset button to submit button
                btnSubmit.children().text("Submit");
                // disable submit button
                btnSubmit.prop("disabled", true);
                // disable feedback button
                disableFeedback(e);
                // enable All multiple choice
                enableChoice(e);
            } else if ($.trim($(e.target).children().text()) == "Close") {
                $("#myModal-" + this.jasonObj.divId).modal("hide");
            } else if ($.trim($(e.target).children().text()) == "Show Feedback") {
                $("#ev-prompt-body-" + this.jasonObj.divId).empty();
                $("#ev-prompt-body-" + this.jasonObj.divId).append(jasonObj.feedback);
                $("#myModal-" + this.jasonObj.divId).modal("show");
            } else if ($.trim($(e.target).children().text()) == "Show Answer") {
                showCorrectAnswer();
            } else if ($.trim($(e.target).children().text()) == "Hide Answer") {
                hideCorrectAnswer();
            }
        });

        ////////////////////USMAN
        let spaceFound = 0;
        if (autoTrigger = 0) {
            
            Array(length)
                .fill()
                .map((item, i) => {
                    if (this.jasonObj.question[i].selected == "" || this.jasonObj.question[i].selected == null) {
                        spaceFound = 1;
                    }
                })
                .join("");
          
        }

        if (spaceFound == 0) {
            
            if (parseInt(remainingAttempts) != 3) {
                Array(length)
                    .fill()
                    .map((item, i) => {
                        if (this.jasonObj.question[i].selected == "true" || this.jasonObj.question[i].selected == "false") {
                            $("#inid" + (i + 1) + "-" + questionID).val(this.jasonObj.question[i].selected);
                            //select Radio Button
                            //  $("#" + this.jasonObj.divId + "-true").append(
                            //      $("#" + this.jasonObj.divId + "-parentDiv")
                            //          .children()
                            //          .eq(0)
                            // );
                        }// else if (this.jasonObj.question[i].selected == "false") {
                        //   //select Radio Button
                        //   $("#" + this.jasonObj.divId + "-false").append(
                        //       $("#" + this.jasonObj.divId + "-parentDiv")
                        //           .children()
                        //           .eq(0)
                        //   );
                        //}
                    })
                    .join("");
                autoTrigger = 1;
                remainingAttempts++;
                btnSubmit = $("#" + this.jasonObj.divId + "-submit");
                btnSubmit.prop("disabled", false);
                $("button." + this.jasonObj.divId).trigger("click");
                $("#myModal-" + this.jasonObj.divId).modal("hide");
            }
           
        }

        ////////////////////USMAN
    };

    let isTrueAnswerCorrect = () => {
        let trueList = new Array();
        let resultCounter = 0;
        $("#" + this.jasonObj.divId + "-true button").each((i, li) => {
            trueList.push($(li));
        });
        if (trueList.length == trueAnswer.length) {
            if (trueList.length !== 0) {
                for (let i = 0; i < trueList.length; i++) {
                    for (let j = 0; j < trueAnswer.length; j++) {
                        if ($.trim(trueList[i].children("span").text()) == trueAnswer[j])
                            resultCounter++;
                    }
                }
                return trueAnswer.length == resultCounter ? true : false;
            }
            return false;
        }
        return false;
    };

    let isFalseAnswerCorrect = () => {
        let falseList = new Array();
        let resultCounter = 0;
        $("#" + this.jasonObj.divId + "-false button").each((i, li) => {
            falseList.push($(li));
        });
        if (falseList.length == falseAnswer.length) {
            if (falseList.length !== 0) {
                for (let i = 0; i < falseList.length; i++) {
                    for (let j = 0; j < falseAnswer.length; j++) {
                        if ($.trim(falseList[i].children("span").text()) == falseAnswer[j])
                            resultCounter++;
                    }
                }
                return falseAnswer.length == resultCounter ? true : false;
            }
            return false;
        }
        return false;
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
            .text(" final attempt");
    };

    let enableReset = () => {
        btnSubmit.children().text("Reset");
    };

    let enableHideAnswer = () => {
        btnSubmit.children().text("Hide Answer");
    };

    let enableShowAnswer = () => {
        btnSubmit.children().text("Show Answer");
    };

    let hideCorrectAnswer = () => {
        for (var i = 1; i <= length; i++) {

            $("#inid" + i + "-" + questionID).val(falAnswer[i - 1]);
        }
        disableinput();
        enableShowAnswer();
    };

    let showCorrectAnswer = () => {

        for (var i = 1; i <= length; i++) {


            falAnswer.push($("#inid" + i + "-" + questionID).val());
            $("#inid" + i + "-" + questionID).val(this.jasonObj.question[i - 1].type);

        }
        disableinput();
        enableHideAnswer();
    };

    let incorrectAnswer = () => {
        let trueList = new Array();
        $("#" + this.jasonObj.divId + "-true button").each((i, li) => {
            trueList.push($(li));
        });
        for (let i = 0; i < trueList.length; i++) {
            for (let j = 0; j < trueAnswer.length; j++) {
                if ($.trim(trueList[i].children("span").text()) == trueAnswer[j]) {
                    trueList[i].children("i").removeClass("ev-icon-move");
                    trueList[i].children("i").removeClass("ev-icon-cross");
                    trueList[i].children("i").addClass("ev-icon-check");
                    break;
                } else {
                    trueList[i].children("i").removeClass("ev-icon-move");
                    trueList[i].children("i").addClass("ev-icon-cross");
                }
            }
        }

        let falseList = new Array();
        $("#" + this.jasonObj.divId + "-false button").each((i, li) => {
            falseList.push($(li));
        });
        for (let i = 0; i < falseList.length; i++) {
            for (let j = 0; j < falseAnswer.length; j++) {
                if ($.trim(falseList[i].children("span").text()) == falseAnswer[j]) {
                    falseList[i].children("i").removeClass("ev-icon-move");
                    falseList[i].children("i").removeClass("ev-icon-cross");
                    falseList[i].children("i").addClass("ev-icon-check");
                    break;
                } else {
                    falseList[i].children("i").removeClass("ev-icon-move");
                    falseList[i].children("i").addClass("ev-icon-cross");
                }
            }
        }
        disableChoice();
    };

    let correctAnswer = (e) => {
        let setTrueClass = $("#" + this.jasonObj.divId + "-true button").children(
            "i"
        );
        let setFalseClass = $("#" + this.jasonObj.divId + "-false button").children(
            "i"
        );
        setTrueClass.removeClass("ev-icon-move");
        setTrueClass.addClass("ev-icon-check");

        setFalseClass.removeClass("ev-icon-move");
        setFalseClass.addClass("ev-icon-check");
        disableChoice();
    };

    let disableChoice = () => {
        // siblingsOfLabel.addClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-true").addClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-false").addClass("ev-disabled");
        $("#" + this.jasonObj.divId + "-true button").attr("draggable", "false");
        $("#" + this.jasonObj.divId + "-false button").attr("draggable", "false");
        $("#" + this.jasonObj.divId + "-parentDiv").attr("draggable", "false");
        $("#" + this.jasonObj.divId + "-true button").css("cursor", "default");
        $("#" + this.jasonObj.divId + "-false button").css("cursor", "default");
        // $("." + this.jasonObj.divId + "-li").off("click");
    };

    let enableChoice = (e) => {
        for (var i = 1; i <= length; i++) {

            $("#inid" + i + "-" + questionID).val('');
        }

        ///  $("#" + this.jasonObj.divId + "-true").removeClass("ev-disabled");
        ///  $("#" + this.jasonObj.divId + "-false").removeClass("ev-disabled");
        ///  $("#" + this.jasonObj.divId + "-true button").attr("draggable", "true");
        ///  $("#" + this.jasonObj.divId + "-false button").attr("draggable", "true");
        ///  $("#" + this.jasonObj.divId + "-parentDiv").attr("draggable", "true");
        ///  $("#" + this.jasonObj.divId + "-true button").css("cursor", "move");
        ///  $("#" + this.jasonObj.divId + "-false button").css("cursor", "move");
        ///  $("#" + this.jasonObj.divId + "-true button").remove();
        ///  $("#" + this.jasonObj.divId + "-false button").remove();
        ///  $("#" + this.jasonObj.divId + "-parentDiv").empty();
        ///  $("#" + this.jasonObj.divId + "-parentDiv").append(choices);
        // $("." + this.jasonObj.divId + "-li").on("click", (e) => {
        //   setTimeout(() => {
        //     questionLength = $("#" + this.jasonObj.divId + "-question button")
        //       .length;
        //     if (questionLength == 0) {
        //       btnSubmit = $("#" + this.jasonObj.divId + "-submit");
        //       btnSubmit.prop("disabled", false);
        //     }
        //   }, 600);
        // });
    };

    let hideAttempts = (e) => {
        $(e.target).parent().siblings().css("visibility", "hidden");
    };

    let disableSubmit = () => {
        btnSubmit.prop("disabled", true);
    };
    let disableinput = () => {
        for (var i = 1; i <= length; i++) {
            $("#inid" + i + "-" + questionID).prop("disabled", true);
        }

    };

    let enableFeedback = (e) => {
        $(e.target).siblings("button.ev-buttons-feedback").prop("disabled", false);
    };

    let disableFeedback = (e) => {
        $(e.target).siblings("button.ev-buttons-feedback").prop("disabled", true);
    };

    let incorrectIcon = (e) => {
        $(e.target).siblings("div.ev-icon-cross").css("display", "block");
    };

    let correctIcon = (e) => {
        $(e.target).siblings("div.ev-icon-check").css("display", "block");
    };
    let PushDataToDB = () => {
        //if (remainingAttempts <= 0) {
        //    trueAnswerList = new Array();
        //    $("#myModal-" + this.jasonObj.divId).modal("hide");
        //    return false;
        //}
        var questionId = this.jasonObj.divId.split('-')[1]
        if (autoTrigger == 1) {
            autoTrigger = 0;
        }
        else {
            var ModuleId = $("#hdnModuleId").val();
            var CourseId = $("#hdnCourseId-" + questionId).val();
            var freetext = "";
            var ApplicatiionPath = $("#hdnApplicatonPath").val();
            if (ApplicatiionPath == "") {
                hr = window.location.href.split("/");
                ApplicatiionPath = hr[0] + "//" + hr[2];
            }
            var url = ApplicatiionPath + '/Users/Courses/SubmitAnswer';
            var requestPerameters = {
                "id": ModuleId,
                "course": CourseId,
                "answer": trueAnswerList,
                "question": questionId,
                "flagged": false,
 		"FreeTextAnswer": freetext
            }
            AjaxPostRequestWithRequestPerameters(url, requestPerameters, function (response) {
                if (response.Feedback) {
                    if (response.IsComplete) {
                        console.log("Alhumdulillah");
                        var nextModuleId = $("#hdnNextModuleId").val();
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

        }
    }
    $("#inid" + "-" + questionID).on('change', function () {
        btnSubmit.prop("enabled", true);
    })
    
}


