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
                sh "docker run --detach --name kuchnia --network kuchnia -p 12038:8080 -v /var/lib/docker/volumes/kuchnia_config/_data/config.json:/app/appsettings.Production.json ${imageTag}"
            }
        }

        stage('Install Postman CLI') {
            steps {
                sh 'curl -o- "https://dl-cli.pstmn.io/install/linux64.sh" | sh'
            }
        }

        stage('Postman CLI Login') {
            steps {
                sh 'postman login --with-api-key $POSTMAN_API_KEY'
                }
        }

        stage('Run tests') {
            steps {
                sh "/var/jenkins_home/scripts/run_tests.sh"
            }
        }
    }
}
