FROM mariadb:latest

ARG dbname_prefix=planningpoker

COPY ["./orleanssql/", "/orleanssql"]

WORKDIR "/docker-entrypoint-initdb.d"
RUN printf "create database ${dbname_prefix}_grains;\nuse ${dbname_prefix}_grains;\n\n" > ${dbname_prefix}_grains.sql
RUN cat /orleanssql/MySQL-Main.sql >> ${dbname_prefix}_grains.sql
RUN cat /orleanssql/MySQL-Persistence.sql >> ${dbname_prefix}_grains.sql

RUN printf "create database ${dbname_prefix}_clustering;\nuse ${dbname_prefix}_clustering;\n\n" > ${dbname_prefix}_clustering.sql
RUN cat /orleanssql/MySQL-Main.sql >> ${dbname_prefix}_clustering.sql
RUN cat /orleanssql/MySQL-Clustering.sql >> ${dbname_prefix}_clustering.sql

RUN printf "create database ${dbname_prefix}_pubsub;\nuse ${dbname_prefix}_pubsub;\n\n" > ${dbname_prefix}_pubsub.sql
RUN cat /orleanssql/MySQL-Main.sql >> ${dbname_prefix}_pubsub.sql
RUN cat /orleanssql/MySQL-Persistence.sql >> ${dbname_prefix}_pubsub.sql

RUN rm /orleanssql -d -r

WORKDIR .

ENTRYPOINT ["docker-entrypoint.sh"]

EXPOSE 3306

CMD ["mariadbd"]