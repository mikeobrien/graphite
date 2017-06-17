var gulp = require('gulp'),
    args = require('yargs').argv,
    assemblyInfo = require('gulp-dotnet-assembly-info'),
    msbuild = require('gulp-msbuild'),
    nunit = require('gulp-nunit-runner'),
    nuget = require('nuget-runner')();

gulp.task('deploy', ['nuget-push']);

gulp.task('ci', ['nuget-pack']);

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
                framework: 'net-4.5',
                result: 'TestResults.xml'
            }
        }));
});

gulp.task('nuget-copy-graphite', ['test'], function() {
    return gulp.src('src/Graphite/bin/Release/Graphite.{dll,pdb,xml}')
        .pipe(gulp.dest('graphite-package/lib'));
});

gulp.task('nuget-copy-structuremap', ['test'], function() {
    return gulp.src('src/Integration/DependencyInjection/Graphite.StructureMap' + 
            '/bin/Release/Graphite.StructureMap.{dll,pdb,xml}')
        .pipe(gulp.dest('structuremap-package/lib'));
});

gulp.task('nuget-copy-bender', ['test'], function() {
    return gulp.src('src/Integration/Serializers/Graphite.Bender/bin/' + 
            'Release/Graphite.Bender.{dll,pdb,xml}')
        .pipe(gulp.dest('bender-package/lib'));
});

gulp.task('nuget-pack-graphite', ['nuget-copy-graphite'], function() {

    return nuget.pack({
        spec: 'GraphiteWeb.nuspec',
        basePath: 'graphite-package',
        version: args.buildVersion
    });
});

gulp.task('nuget-pack-structuremap', ['nuget-copy-structuremap'], function() {
    return nuget.pack({
        spec: 'GraphiteWeb.StructureMap.nuspec',
        basePath: 'structuremap-package',
        version: args.buildVersion
    });
});

gulp.task('nuget-pack-bender', ['nuget-copy-bender'], function() {
    return nuget.pack({
        spec: 'GraphiteWeb.Bender.nuspec',
        basePath: 'bender-package',
        version: args.buildVersion
    });
});

gulp.task('nuget-pack', ['nuget-pack-graphite', 'nuget-pack-structuremap', 
    'nuget-pack-bender']);

gulp.task('nuget-push', ['nuget-pack'], function() {
    return nuget.push('*.nupkg', { 
        apiKey: args.nugetApiKey, 
        source: ['https://www.nuget.org/api/v2/package'] 
    });
});
