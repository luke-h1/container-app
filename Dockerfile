FROM node:16 as builder

WORKDIR /usr/src/app

COPY package*.json ./
COPY pnpm-lock.yaml ./

COPY . .
RUN npm i -g pnpm
RUN pnpm install
RUN pnpm --filter=explore-education-statistics-frontend build
RUN pnpm --filter=explore-education-statistics-frontend deploy public-frontend

WORKDIR /usr/src/app/public-frontend

ENV NODE_ENV=production
EXPOSE 3000

CMD [ "pnpm", "start" ]
USER node
