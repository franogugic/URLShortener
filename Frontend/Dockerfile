# 1. Stage: Build
FROM node:20-alpine AS build

# Postavi radni direktorij
WORKDIR /app

# Kopiraj package files i instaliraj dependencije
COPY package*.json ./
RUN npm install

# Kopiraj sav izvorni kod
COPY . .

# Pokreni build aplikacije (ovo stvara 'dist' folder)
RUN npm run build

# 2. Stage: Serve (Nginx)
FROM nginx:stable-alpine

# Kopiraj tvoju custom nginx konfiguraciju
# PAZI: nginx.conf mora biti u istom folderu kao i Dockerfile na Macu
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Kopiraj buildane fileove iz prvog stage-a u nginx folder
COPY --from=build /app/dist /usr/share/nginx/html

# Port na kojem Nginx sluša
EXPOSE 80

# Pokreni Nginx
CMD ["nginx", "-g", "daemon off;"]