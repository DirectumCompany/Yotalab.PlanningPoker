FROM mariadb:latest
WORKDIR "/docker-entrypoint-initdb.d"
WORKDIR .
ENTRYPOINT ["docker-entrypoint.sh"]
EXPOSE 3306
CMD ["mariadbd"]