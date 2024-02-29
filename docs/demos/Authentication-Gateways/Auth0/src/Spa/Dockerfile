FROM node:latest as build

WORKDIR /src
COPY . .

WORKDIR /src/Spa

RUN npm install

RUN npm run build


FROM nginx:latest

COPY --from=build /src/dist/test-app /usr/share/nginx/html

EXPOSE 80