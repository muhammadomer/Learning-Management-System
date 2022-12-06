/// <binding ProjectOpened='watch-sass-less, sass-less-compile' />
var gulp = require('gulp');
var sass = require('gulp-sass');
var less = require('gulp-less');
var del = require('del');
var checkVSIncludes = require('check-vs-includes');

var srcSCSSDir = './_Content/css/scss/';
var srcLessDir = './_Content/css/less/';
var destCssDir = './_Content/css/';

//Comma Seperate your exclude list.
var excludeList = '!./_Content/css/scss/none.scss';

gulp.task('sass-less-compile', ['clean'], function () {
    gulp.src([srcSCSSDir + '*.scss', excludeList])
       .pipe(sass().on('error', sass.logError))
       .pipe(gulp.dest(destCssDir));

    gulp.src(srcLessDir + '*.less')
        .pipe(less())
        .pipe(gulp.dest(destCssDir));
});

gulp.task('clean', function (cb) {
    del([destCssDir + '*.css']).then(function (paths) {
        cb();
    });
});

gulp.task('watch-sass-less', function () {

    var scssFiles = srcSCSSDir + '*.scss';
    var lessFiles = srcLessDir + '*.less';

    gulp.watch([srcSCSSDir + '*.scss', srcLessDir + '*.less'], ['sass-less-compile']);

    console.log("Watching Directory - " + scssFiles);
    console.log("Watching Directory - " + lessFiles);
});

gulp.task('check-vs-includes', function (cb) {
    checkVSIncludes(['./_Content/css/*.css']);
});