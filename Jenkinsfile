node {
	stage 'Checkout'
	    checkout scm

	stage 'Build'
	    bat "\"dotnet\" restore \"${workspace}/TaskPipeline.sln\""
	    bat "\"dotnet\" build \"${workspace}/TaskPipeline.sln\""

	stage 'UnitTests'
	    bat returnStatus: true, script: "\"dotnet\" test \"${workspace}/TaskPipeline.sln\" --logger \"trx;LogFileName=unit_tests.xml\" --no-build"
	    step([$class: 'MSTestPublisher', testResultsFile:"**/unit_tests.xml", failOnError: true, keepLongStdio: true])
}