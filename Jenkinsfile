node {
	stage 'Checkout'
		checkout scm

	stage 'Build'
		bat 'nuget restore TaskPipeline.sln'
		bat "\"${tool 'MSBuild'}\" TaskPipeline.sln /p:Configuration=Release /p:Platform=\"Any CPU\" /p:ProductVersion=1.0.0.${env.BUILD_NUMBER}"

	stage 'Archive'
		archive 'TaskPipeline/bin/Release/**'

}