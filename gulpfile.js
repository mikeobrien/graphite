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
            toolsVersion: 15.0,
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
                result: 'TestResults.xml',
                where: 'cat != Performance'
            }
        }));
});

function buildNugetPackage(assembly, packageName)
{
    var source = 'src/' + assembly + '/bin/Release/' + 
    	assembly + '.{dll,pdb,xml}';
    var dest = packageName + '-package/lib';

    gulp.task('nuget-copy-' + packageName, ['test'], function() {
    	console.log('Copying ' + source + ' to ' + dest + '...');
        return gulp.src(source).pipe(gulp.dest(dest));
    });

    gulp.task('nuget-pack-' + packageName, ['nuget-copy-' + packageName], function() {
    	var spec = packageName + '.nuspec';
    	var path = packageName + '-package';
    	console.log('Packing ' + path + ' for ' + spec + '...');
        return nuget.pack({
            spec: spec,
            basePath: path,
            version: args.buildVersion
        });
    });
}

buildNugetPackage('Graphite', 'GraphiteWeb.Core');
buildNugetPackage('Graphite.AspNet', 'GraphiteWeb.AspNet');
buildNugetPackage('Graphite.Owin', 'GraphiteWeb.Owin');
buildNugetPackage('Graphite.StructureMap', 'GraphiteWeb.StructureMap');
buildNugetPackage('Graphite.Cors', 'GraphiteWeb.Cors');

gulp.task('nuget-pack', [
    'nuget-pack-GraphiteWeb.Core',
    'nuget-pack-GraphiteWeb.AspNet', 
    'nuget-pack-GraphiteWeb.Owin', 
    'nuget-pack-GraphiteWeb.StructureMap', 
    'nuget-pack-GraphiteWeb.Cors']);

gulp.task('nuget-push', ['nuget-pack'], function() {
    return nuget.push('*.nupkg', { 
        apiKey: args.nugetApiKey, 
        source: ['https://www.nuget.org/api/v2/package'] 
    });
});
