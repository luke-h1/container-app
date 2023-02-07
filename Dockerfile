FROM node:16-alpine as builder

WORKDIR /app

COPY package*.json ./
COPY pnpm-lock.yaml ./

COPY . .
RUN corepack enable
RUN pnpm i

RUN pnpm --filter=explore-education-statistics-frontend build
RUN pnpm --filter=explore-education-statistics-frontend deploy public-frontend --prod

WORKDIR /app/public-frontend

ENV NODE_ENV=production
EXPOSE 3000

CMD [ "pnpm", "start" ]
USER node
