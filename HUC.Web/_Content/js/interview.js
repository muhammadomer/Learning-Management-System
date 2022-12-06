// constructor function
function Interview(jasonObj) {
  this.jasonObj = jasonObj;
  let index = 0;

  this.initialize = () => {
    let module = `
    <div class="ev-component ev-dialog-component">
        <!-- fade mouse icon -->
        <div class="ev-component-interaction-label ev-is-incomplete mouse-icon">
          <div class="ev-interaction-label-inner">
            <index
              class="ev-icon ev-interaction-label-icon ev-icon-mouse-left">
            </index>
          </div>
        </div> 
        <!-- star icon when completed -->
        <div class="ev-component-interaction-label ev-is-complete star-icon" style="display: none;">
          <div class="ev-interaction-label-inner">
            <index
              class="ev-icon ev-interaction-label-icon ev-icon-star"
            ></index>
          </div>
        </div>
        <div class="ev-dialog-widget ev-component-widget ev-clearfix">
          <!-- start dialog button -->
          <div>
            <div class="ev-dialog-start-avatars" style="transform: translate3d(0px, 0%, 0px);opacity: 1;">
              <div class="ev-dialog-start-avatar" style="background-color: rgb(153, 255, 204);">
                <img class="ev-img" src="../../../_Content/images/interviewer.png" alt="Interviewer" style="width: 50px;"/>
                <div class="ev-dialog-start-name" style="color: rgb(0, 0, 0);">
                  Interviewer
                </div>
              </div>
              <div class="ev-dialog-start-avatar" style="background-color: rgb(153, 204, 255);">
                <img class="ev-img" src="../../../_Content/images/expert.png" alt="Expert" style="width: 50px;"/>
                <div class="ev-dialog-start-name" style="color: rgb(0, 0, 0);">
                  Expert
                </div>
              </div>
            </div>
            <div class="ev-dialog-start-action">
              <button class="ev-button dialog-btn" id=${this.jasonObj.divId}>
                Start Dialog
              </button>
            </div>
          </div>
          <ul>
            <!-- start dialog list -->
          </ul>
          <!-- next button -->
          <div class="" style="text-align: left; display: none;">
            <button type="button" class="ev-button ev-has-icon next-btn" id=${this.jasonObj.divId}>
              <span class="ev-button-inner">
                <index class="ev-icon ev-icon-play-circle"></index>
              <span>Next</span>
              </span>
            </button>
          </div>
          <div class="scroll" id=${this.jasonObj.divId} style="display: none;"></div>
        </div>
    </div>
    `;

    $(".interview-module#" + this.jasonObj.divId).append(module);

    $(document).on("click", ".dialog-btn#" + this.jasonObj.divId, (event) => {
      $(event.target).parent().parent().hide();
      $(event.target).parent().parent().siblings("div").show();
      getList($(event.target).parent().parent().siblings("ul"));
      $(event.target).parent().parent().siblings("div").children().focus();
    });

    $(document).on("click", ".next-btn#" + this.jasonObj.divId, (event) => {
      if (this.jasonObj.dialogs.length - 1 === index) {
        $(event.target).parent().hide();
        $(event.target).parent().parent().siblings(".mouse-icon").hide();
        $(event.target).parent().parent().siblings(".star-icon").show();
      }
      getList($(event.target).parent().siblings("ul"));

      //$("html, body").animate(
      //  {
      //    scrollTop: $(".scroll#" + this.jasonObj.divId).offset()
      //      .top,
      //  },
      //  100
      //);
    });
  };

  let getList = (listEvent) => {
    let interviewer = `
    <li class="ev-dialog-item" style="opacity: 1; transform: none; width: 100%; animation: left_to_right 0.5s;">
      <div class="ev-dialog-item-inner" style="background-color: rgb(153, 255, 204); width: 80%;">
        <div class="ev-dialog-item-avatar">
            <img src="../../../_Content/images/interviewer.png" alt="Interviewer" style="width: 50px;"/>
        </div>
        <div class="ev-dialog-item-message"style="color: rgb(0, 0, 0);">
          <p> ${this.jasonObj.dialogs[index].message.replace("[name]", this.jasonObj.name).replace("[role]", this.jasonObj.role).replace("[place]", this.jasonObj.place)} </p>
        </div>
      </div>
    </li>`;
    let expert = `
    <li class="ev-dialog-item ev-dialog-item-right-item" style="width: 100%; animation: right_to_left 0.5s;">
      <div class="ev-dialog-item-inner" style="background-color: rgb(153, 204, 255); width: 80%;">
        <div class="ev-dialog-item-avatar">
            <img src="../../../_Content/images/expert.png" alt="Expert" style="width: 50px;"/>
        </div>
        <div class="ev-dialog-item-message" style="color: rgb(0, 0, 0);">
          <p>${this.jasonObj.dialogs[index].message.replace("[name]", this.jasonObj.name).replace("[role]", this.jasonObj.role).replace("[place]", this.jasonObj.place)}</p>
        </div>
      </div>
    </li>`;
    this.jasonObj.dialogs[index].type === "expert"
      ? $(listEvent).append(expert)
      : $(listEvent).append(interviewer);
    index += 1;
  };
}
