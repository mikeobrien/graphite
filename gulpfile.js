var gulp = require('gulp'),
    args = require('yargs').argv,
    assemblyInfo = require('gulp-dotnet-assembly-info'),
    msbuild = require('gulp-msbuild'),
    nunit = require('gulp-nunit-runner'),
    nuget = require('nuget-runner')();

gulp.task('deploy', ['nuget-push']);

gulp.task('ci', ['nuget-package']);

gulp.task('assemblyInfo', function() {
    return gulp
        .src('**/AssemblyInfo.cs')
        .pipe(assemblyInfo({
            version: args.buildVersion,
            fileVersion: args.buildVersion,
            copyright: function(value) {
                return 'Copyright © ' + new Date().getFullYear() + ' Setec Astronomy.';
            }
        }))
        .pipe(gulp.dest('.'));
});

gulp.task('packageRestore', ['assemblyInfo'], function() {

    return nuget.restore({
        source: ['https://www.nuget.org/api/v2'],
        packages: 'src/Graphite.sln'
    });
});

gulp.task('build', ['packageRestore'], function() {
    return gulp
        .src('src/*.sln')
        .pipe(msbuild({
            toolsVersion: 14.0,
            targets: ['Clean', 'Build'],
            errorOnFail: true,
            stdout: true
        }));
});

gulp.task('test', ['build'], function () {
    return gulp
        .src(['**/bin/**/*Tests.dll'], { read: false })
        .pipe(nunit({
            executable: 'nunit3-console.exe',
            teamcity: true,
            options: {
                framework: 'net-4.5'
            }
        }));
});

gulp.task('nuget-lib', ['test'], function() {
    gulp.src('src/Graphite/bin/Release/Graphite.{dll,pdb,xml}')
        .pipe(gulp.dest('package/lib'));

    gulp.src('src/Integration/DependencyInjection/Graphite.StructureMap' + 
            '/bin/Release/Graphite.StructureMap.{dll,pdb,xml}')
        .pipe(gulp.dest('structuremap-package/lib'));

    gulp.src('src/Integration/Serializers/Graphite.Bender/bin/' + 
            'Release/Graphite.Bender.{dll,pdb,xml}')
        .pipe(gulp.dest('bender-package/lib'));

    gulp.src('src/Integration/Serializers/Graphite.Newtonsoft.Json/' + 
            'bin/Release/Graphite.Newtonsoft.Json.{dll,pdb,xml}')
        .pipe(gulp.dest('jsonnet-package/lib'));
});


gulp.task('nuget-package', ['nuget-lib'], function() {

    nuget.pack({
        spec: 'Graphite.nuspec',
        basePath: 'package',
        version: args.buildVersion
    });

    nuget.pack({
        spec: 'Graphite.StructureMap.nuspec',
        basePath: 'structuremap-package',
        version: args.buildVersion
    });

    nuget.pack({
        spec: 'Graphite.Bender.nuspec',
        basePath: 'bender-package',
        version: args.buildVersion
    });

    nuget.pack({
        spec: 'Graphite.Newtonsoft.Json.nuspec',
        basePath: 'jsonnet-package',
        version: args.buildVersion
    });
});

gulp.task('nuget-push', ['nuget-package'], function() {
    return nuget.push('*.nupkg', { 
        apiKey: args.nugetApiKey, 
        source: ['https://www.nuget.org/api/v2/package'] 
    });
});
