FROM node:16 as builder

WORKDIR /usr/src/app

COPY package*.json ./
COPY pnpm-lock.yaml ./

COPY . .
RUN npm i -g pnpm
RUN pnpm install
RUN pnpm --filter=explore-education-statistics-frontend build
RUN pnpm --filter=explore-education-statistics-admin deploy public-frontend

# stage 2
FROM node:16

WORKDIR /usr/src/app

COPY --from=builder /usr/src/app/public-frontend .

COPY package*.json ./
COPY pnpm-lock.yaml ./

RUN npm i -g pnpm
RUN pnpm install

ENV NODE_ENV=production
EXPOSE 3000

CMD [ "pnpm", "start" ]
USER node
