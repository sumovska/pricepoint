/** Source paths **/
var src = {
  root: 'src/',
  html: 'src/',
  css: 'src/css/',
  less: 'src/less/',
  js: 'src/js/',
  vendor: 'bower_components/',
  img: 'src/img/',
  svg: 'src/img/svg/',
  fonts: 'src/fonts/',
  video: 'src/video/'
};

/** Destination paths **/
var dist = {
  root: 'dist/',
  html: 'dist/',
  css: 'dist/css/',
  js: 'dist/js/',
  img: 'dist/img/',

  fonts: 'dist/fonts/',
  video: 'dist/video/'
};


var config = {
  src: 'src/',
  dest: 'dist/'
};


module.exports = function (grunt) {
  grunt.initConfig({
    config: config,

    clean: {
      pre: [dist.root, src.css, src.js + 'vendor'],
      after: [src.css + 'temp'],
      dist: [dist.js + 'custom.js']
    },
    copy: {
      dist: {
        files: [
          {
            expand: true,
            cwd: '<%= config.src %>',
            dest: '<%= config.dest %>',
            src: [
              'css/{,*/}*.css',
              'img/**/*',
              'js/{,*/}*.js',
              'fonts/{,*/}*.*',
              'video/{,*/}*.*',
              '{,*/}*.html'
            ]
          }
        ]
      }
    },
    concat: {
      options: {
        separator: '\n\n\n'
      },
      dist: {
        files: [{
            src: [
              src.vendor + 'pure/pure-min.css'
            ],
            dest: src.css + 'pure.css'
          }
        ]
      }
    },
    cssmin: {
      options: {
        separator: '\n\n\n'
      },
      dist: {
        files: [
          {
            expand: true,
            flatten: true,
            src: src.css + 'temp/*.css',
            dest: src.css + 'vendor',
            ext: '.min.css'
          }
        ]
      }
    },
    less: {
      dev: {
        options: {
          paths: [src.less]
        },
        files: [{
          expand: true,
          cwd: src.less,
          src: ["**/*.less"],
          dest: src.css,
          ext: ".css"
        }]
      }
    },
    watch: {
      options: {
        livereload: true
      },
      scripts: {
        files: [src.js + "*.js"],
        tasks: ["process"]
      },
      styles: {
        files: [src.less + "*.less"],
        tasks: ["process"]
      },
      html: {
        files: [src.html + "*.html"],
        tasks: ["process"]
      },
      images: {
        files: [src.img + "**/*.*"],
        tasks: ["process"]
      }
    }
  });

  grunt.loadNpmTasks("grunt-contrib-clean");
  grunt.loadNpmTasks("grunt-contrib-less");
  grunt.loadNpmTasks("grunt-contrib-cssmin");
  grunt.loadNpmTasks("grunt-contrib-copy");
  grunt.loadNpmTasks("grunt-contrib-rename");
  grunt.loadNpmTasks("grunt-contrib-concat");
  grunt.loadNpmTasks("grunt-contrib-uglify");
  grunt.loadNpmTasks("grunt-contrib-watch");
  grunt.loadNpmTasks("grunt-newer");

  grunt.registerTask("default", ["clean:pre", "less", "cssmin", "concat", "clean:after", "copy:dist", "clean:dist", "watch"]);
  grunt.registerTask("process", ["newer:less", "newer:copy:dist"]);
};