imageTag = "reservant-api:${env.BRANCH_NAME.replaceAll('[^a-zA-Z0-9]', '_')}"

pipeline {
    agent any


    environment {
        POSTMAN_API_KEY = credentials('jenkins-postman-api-key')
    }

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
                sh "docker rm kuchnia || true"
                sh "docker run --detach \
                        --name kuchnia \
                        --network kuchnia \
                        -p 12038:8080 \
                        -v /var/lib/docker/volumes/kuchnia_config/_data/config.json:/app/appsettings.Production.json \
                        -v /var/lib/docker/volumes/kuchnia_config/_data/firebase-credentials.json:/app/firebase-credentials.json \
                        ${imageTag}"
            }
        }
        stage ('Run tests') {
            when {
                branch 'main'
            }
            steps {
                build job: 'Reservant Backend-Tests/main', parameters: [string(name: 'label_string', value: 'BACKEND DEPLOY')]
            }
        }
    }
}
