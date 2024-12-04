function Flip(jasonObj) {
    this.jasonObj = jasonObj;
    console.log(jasonObj);
    let length = this.jasonObj.flip.length;
    console.log(jasonObj);
    var flipar = JSON.stringify(jasonObj.flip);
    let flipCounter = 0;
    this.initialize = () => {
        let list = `${Array(length)
            .fill()
            .map(
                (item, i) => `
<div class="col-md-6">
          <li class="ev-grid-item" id="litem-${this.jasonObj.divId}-${i}"  style=" flex: 1 1 33.3333%; text-align: center; box-sizing: border-box; margin: 10px auto">
            <a onclick="DeleteConfirm('${this.jasonObj.flip[i].IndexId}','${this.jasonObj.divId}' ,' ${encodeURIComponent(JSON.stringify(this.jasonObj.flip))}','${i}','${this.jasonObj.ID}','${this.jasonObj.ChapterID}','${this.jasonObj.Sort}');" class="trigger-btn"><i class="fa fa-close close-btn" ></i></a>      
            <a style="margin-right:10px;" onclick="ContentFlipEdit('${this.jasonObj.flip[i].IndexId}','${this.jasonObj.divId}' ,' ${encodeURIComponent(JSON.stringify(this.jasonObj.flip))}','${i}','${this.jasonObj.ID}','${this.jasonObj.ChapterID}','${this.jasonObj.Sort}');" class="trigger-btn"><i class="fa fa-pencil close-btn" ></i></a>
<div class="ev-grid-item-inner flip-container" style="padding: 0px 10px; box-sizing: border-box;">
              <div class="ev-flip-card-item flipper" tabindex="0">
                  <div class="ev-flip-card-item-front" style="height:310px;width:440px;vertical-align: middle;top:50%;bottom:50%" >
${this.jasonObj.flip[i].heading ? `<div class="ev-flip-card-item-front-image"  style="height:310px;width:440px;background-color:${this.jasonObj.flip[i].heading};display:flex;" >
                         <div style="text-align: center;vertical-align: middle;padding:12px;margin:auto;color:#000000 !Important;" >
                             ${this.jasonObj.flip[i].content} 
                        </div>
                      </div>`: `<div class="ev-flip-card-item-front-image"  style="height:310px;width:440px;" >
                         <div>
                             <img src="${this.jasonObj.flip[i].imagePath}" height="310" width="440" style="height:310px !important;width:440px !important;"  />
                        </div> 
                      </div>`}
                      
                      <div class="ev-flip-card-item-front-inner ev-is-center-alignment" style="overflow-y: auto;">
 <p style="position:absolute;top: 10px;left: 10px;color:lightgray">Click to Flip</p>
                          <div class="ev-flip-card-item-content"></div>
                      </div>
                  </div>
                  <div class="ev-flip-card-item-back"  style="height:310px;width:440px;  transform: rotateY(180deg);">
				  <style>div#backcontent * {
    display: contents !important;
}</style>
<div class="ev-flip-card-item-back-inner"  style="height:310px;width:440px;background-color:${this.jasonObj.flip[i].backcolor};display:flex;margin-left: 10px;" >
<p style="position:absolute;top: 10px;left: 10px;color:gray">Click to Flip</p>
                         <div style="text-align: center;vertical-align: middle;padding:12px;margin:auto;color:#000000 !Important;" >
 
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
function DeleteConfirm(IndexId, divid, array, loopid, ID, ChapterId, Sort) {
    debugger;
    results = decodeURIComponent(array);
    $("#DeleteModalFlip").modal('show');
    $("#FlipValue").val(IndexId);
    $("#DivId").val(divid);
    $("#arraydata").val(results);
    var removeli = '#litem-' + divid + "-" + loopid
    $("#liremoveid").val(removeli);
    $("#ID").val(ID);
    $("#ChapterID").val(ChapterId);
    $("#ContentType").val("flip");
    $("#Sort").val(Sort);

    //  $('#litem-'+divid+"-"+loopid).remove();
};

function ContentFlipEdit(IndexId, divid, array, loopid, ID, ChapterId, Sort) {
    debugger;
    results = decodeURIComponent(array);
    sessionStorage.setItem('flipeditindex', IndexId);
    window.location.href = '/company/Resources/ContentEdit/' + ID ;
}

$(document).ready(function () {
    $(".ev-grid-item").mouseover(function () {
        $(".trigger-btn").css("display", "block");
    });
    $(".ev-grid-item").mouseout(function () {
        $(".trigger-btn").css("display", "none");
    });
});



 //<div class="ev-component-interaction-label ev-is-incomplete" style="display: block;">
 //           <div class="ev-interaction-label-inner">
 //               <i class="ev-icon ev-interaction-label-icon ev-icon-mouse-left"></i>
 //               <div class="ev-interaction-label-text">
 //           Click Flip card to read
 //               </div>
 //           </div>
 //       </div>
 //       <div class="ev-component-interaction-label ev-is-complete" style="display: none;">
 //           <div class="ev-interaction-label-inner">
 //               <i class="ev-icon ev-interaction-label-icon ev-icon-star"></i>
 //               <div class="ev-interaction-label-text">
 //               All done!
 //               </div>
 //           </div>
 //       </div>

