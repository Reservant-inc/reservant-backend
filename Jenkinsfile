imageTag = "reservant-api:${env.BRANCH_NAME.replaceAll('[^a-zA-Z0-9]', '_')}"

pipeline {
    agent any

    triggers {
        cron('23 * * * *')
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
    }
}
