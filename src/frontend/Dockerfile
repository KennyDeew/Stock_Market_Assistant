# Этап 1: Сборка
FROM node:22-alpine AS builder

WORKDIR /app

# Копируем package.json для кэширования
COPY package*.json ./
RUN npm ci

# Копируем исходный код
COPY . .

# Используем ARG из docker-compose для передачи переменных
ARG VITE_AUTH_API_URL=/api/auth
ARG VITE_STOCKCARD_API_URL=/api/v1
ARG VITE_PORTFOLIO_API_URL=/api/portfolio
ARG VITE_ANALYTICS_API_URL=/api/analytics
ARG VITE_API_BASE_URL=/


# Создаем .env файл для production
RUN echo "VITE_AUTH_API_URL=${VITE_AUTH_API_URL}" > .env.production
RUN echo "VITE_STOCKCARD_API_URL=${VITE_STOCKCARD_API_URL}" >> .env.production
RUN echo "VITE_PORTFOLIO_API_URL=${VITE_PORTFOLIO_API_URL}" >> .env.production
RUN echo "VITE_ANALYTICS_API_URL=${VITE_ANALYTICS_API_URL}" >> .env.production
RUN echo "VITE_API_BASE_URL=${VITE_API_BASE_URL}" >> .env.production

# Сборка приложения
RUN npm run build

# Этап 2: Production
FROM nginx:alpine

# Копируем собранное приложение
COPY --from=builder /app/dist /usr/share/nginx/html

# Копируем конфигурацию nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Открываем порт
EXPOSE 5273

CMD ["nginx", "-g", "daemon off;"]