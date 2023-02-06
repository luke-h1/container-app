FROM node:16 as builder

WORKDIR /usr/src/app

COPY . .

RUN npm install -g pnpm

RUN pnpm i

RUN pnpm run build --filter explore-education-statistics-frontend

# pnpm deploy
RUN pnpm run deploy --filter explore-education-statistics-frontend --prod public-frontend

# stage 2
FROM node:16 as deployer

WORKDIR /usr/src/app

COPY --from=builder /usr/src/app/public-frontend .

RUN pnpm i

ENV NODE_ENV=production

EXPOSE 3000

CMD ["pnpm", "start"]
USER node
