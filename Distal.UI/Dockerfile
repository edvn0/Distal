FROM node:18 AS build

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm install

COPY . .
RUN npm run build

FROM node:18

WORKDIR /app

COPY --from=build /app/dist /app/dist
COPY --from=build /app/package.json /app/package.json
COPY --from=build /app/server.ts /app/server.ts

RUN npm install --omit=dev

EXPOSE 4000

CMD ["node", "dist/distal.ui/server/server.mjs"]
