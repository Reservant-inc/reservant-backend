pipeline {
    agent any

    stages {
        stage('Build') {
            steps {
                sh "docker build -t reservant-api:${env.BRANCH_NAME} -f Api/Dockerfile ."
            }
        }

        stage('Deploy') {
            when {
                branch 'main'
            }
            steps {
                sh "docker run reservant-api:${env.BRANCH_NAME} --name reservant-api -p 80:8080 --rm"
            }
        }
    }
}
