FROM nginx:alpine

COPY index.html style.css app.js /usr/share/nginx/html/

EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]
