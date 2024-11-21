FROM node:20-slim AS base
ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
RUN corepack enable

WORKDIR /app
COPY . .

FROM base AS prod-deps
RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --prod --frozen-lockfile

FROM base AS build
RUN --mount=type=cache,id=pnpm,target=/pnpm/store pnpm install --frozen-lockfile
RUN pnpm run build

FROM node:20-slim AS runtime
ENV NODE_ENV=production
RUN apt update && apt install -y tree && apt clean && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/dist dist
COPY --from=prod-deps /app/package.json package.json
COPY --from=prod-deps /app/server.ts server.ts

EXPOSE 4000

CMD ["node", "./dist/distal.ui/server/server.mjs"]
