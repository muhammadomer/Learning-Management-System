function Flip(jasonObj) {
    this.jasonObj = jasonObj;
    let length = this.jasonObj.flip.length;
    var flipar = JSON.stringify(jasonObj.flip);
    let flipCounter = 0;
    this.initialize = () => {
        let list = `${Array(length)
            .fill()
            .map(
                (item, i) => `
<div class="col-md-6">
          <li class="ev-grid-item" id="litem-${this.jasonObj.divId}-${i}"  style=" flex: 1 1 33.3333%; text-align: center; box-sizing: border-box; margin: 10px auto">
           <div class="ev-grid-item-inner flip-container" style="padding: 0px 10px; box-sizing: border-box;">
              <div class="ev-flip-card-item flipper" tabindex="0">
                  <div class="ev-flip-card-item-front" style="height:310px;width:440px;vertical-align: middle;top:50%;bottom:50%" >
${this.jasonObj.flip[i].heading ? `<div class="ev-flip-card-item-front-image"  style="height:310px;width:440px;background-color:${this.jasonObj.flip[i].heading};display:flex;" >
                         <div style="text-align: center;vertical-align: middle;padding:12px;margin:auto;color:#000000;" >
                             ${this.jasonObj.flip[i].content} 
                        </div>
                      </div>`: `<div class="ev-flip-card-item-front-image"  style="height:310px;width:440px;" >
                         <div>
                             <img src="${this.jasonObj.flip[i].imagePath}" height="310" width="440" style="height:310px !important;width:440px !important;"  />
                        </div> 
                      </div>`}
                      
                      <div class="ev-flip-card-item-front-inner ev-is-center-alignment" style="overflow-y: auto;">
                          <div class="ev-flip-card-item-content"></div>
                      </div>
                  </div>
                  <div class="ev-flip-card-item-back"  style="height:310px;width:440px;  transform: rotateY(180deg);">
				  <style>div#backcontent * {
    display: contents !important;
}</style>
<div class="ev-flip-card-item-back-inner"  style="height:310px;width:440px;background-color:${this.jasonObj.flip[i].backcolor};display:flex;margin-left: 10px;" >
                         <div style="text-align: center;vertical-align: middle;padding:12px;margin:auto;color:#000000;" >
                            ${this.jasonObj.flip[i].backdescription}
                        </div>
                      </div>
                      
                  </div>
              </div>
          </div>
      </li>
</div>`
            )
            .join("")}`;

        let module = `
    <div class="ev-component">
       
        <ul class="ev-grid">
            ${list}
        </ul>      
    </div>`;

        $(".flip-module#" + this.jasonObj.divId).append(module);
    };

    $(".flip-module#" + this.jasonObj.divId).on("click", function (e) {
        if ($(e.target).parent("div.flipper").css("transform")) {
            $(e.target).parent("div.flipper").css("transform", "rotateY(0deg)");
        } else {
            $(e.target)
                .parent()
                .parent("div.flipper")
                .css("transform", "rotateY(180deg)");
            if (
                $(e.target).parent().parent("div.flipper").attr("tabindex") == "0"
            ) {
                $(e.target).parent().parent("div.flipper").attr("tabindex", "1");
                flipCounter++;
            }
        }

        if (flipCounter == length) {
            $(e.target)
                .parent()
                .parent()
                .parent()
                .parent()
                .parent()
                .siblings("div.ev-is-complete")
                .css("display", "block");

            $(e.target)
                .parent()
                .parent()
                .parent()
                .parent()
                .parent()
                .siblings("div.ev-is-incomplete")
                .css("display", "none");
        }
    });


}
var results;

