FROM mariadb:latest

COPY ["Yotalab.PlanningPoker.Api/OrleansSql/", "/OrleansSql"]

WORKDIR "/docker-entrypoint-initdb.d"
RUN printf 'create database planningpoker_grains;\nuse planningpoker_grains;\n' > planningpoker_grains.sql
RUN cat /OrleansSql/MySQL-Main.sql >> planningpoker_grains.sql
RUN cat /OrleansSql/MySQL-Persistence.sql >> planningpoker_grains.sql

RUN printf 'create database planningpoker_clustering;\nuse planningpoker_clustering;\n' > planningpoker_clustering.sql
RUN cat /OrleansSql/MySQL-Main.sql >> planningpoker_clustering.sql
RUN cat /OrleansSql/MySQL-Clustering.sql >> planningpoker_clustering.sql

RUN printf 'create database planningpoker_pubsub;\nuse planningpoker_pubsub;\n' > planningpoker_pubsub.sql
RUN cat /OrleansSql/MySQL-Main.sql >> planningpoker_pubsub.sql
RUN cat /OrleansSql/MySQL-Persistence.sql >> planningpoker_pubsub.sql

WORKDIR .

ENTRYPOINT ["docker-entrypoint.sh"]

EXPOSE 3306

CMD ["mariadbd"]