function Accordion(jasonObj) {
  this.jasonObj = jasonObj;
  let length = this.jasonObj.accordion.length;

  this.initialize = () => {
    let list = `${Array(length)
      .fill()
      .map(
        (item, i) => `
    <div style='margin-bottom: 3px;'>
      <a href='#' class='ev-accordion-item-title' data-toggle='collapse' data-target='#${this.jasonObj.divId + (i + 1)}'>
        <div class='ev-accordion-item-title-inner'>
          <i class='ev-accordion-item-title-icon ev-icon ev-component-item-title-icon ev-icon-plus'></i>
          <span class='ev-accordion-item-title-text'>
            ${this.jasonObj.accordion[i].heading}
          </span>
        </div>
      </a>
        <div id='${this.jasonObj.divId + (i + 1)}' class='collapse ev-accordion-item-body'>  
          <div class='ev-accordion-item-body-inner'>
            <div class='ev-body'>
              <p>
                <em>${this.jasonObj.accordion[i].content}</em>
              </p>
            </div>
          </div>     
      </div>
    </div>`
      )
      .join("")}`;

    let module = `
    <div class='ev-accordion-component'>
      ${list}
    </div>
    `;
    $(".accordion-module#" + this.jasonObj.divId).append(module);

    $(".collapse").on("show.bs.collapse", function (event) {
      let changeIcon = $(event.target).siblings().children().children("i");
      changeIcon.removeClass("ev-icon-plus");
      changeIcon.addClass("ev-icon-minus");
      $(event.target).siblings("a").addClass("ev-visited");
    });

    $(".collapse").on("hide.bs.collapse", (event) => {
      let changeIcon = $(event.target).siblings().children().children("i");
      changeIcon.removeClass("ev-icon-minus");
      changeIcon.addClass("ev-icon-plus");
    });
  };
}
