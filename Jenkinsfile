imageTag = "reservant-api:${env.BRANCH_NAME.replaceAll('[^a-zA-Z0-9]', '_')}"

pipeline {
    agent any

    stages {
        stage('Build') {
            steps {
                sh "docker build -t ${imageTag} -f Api/Dockerfile ."
            }
        }

        stage('Deploy') {
            when {
                branch 'main'
            }
            steps {
                sh "docker stop kuchnia || true"
                sh "docker run --detach --rm --name kuchnia -p 12038:8080 ${imageTag}"
            }
        }
    }
}
