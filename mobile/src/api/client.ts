import axios, { AxiosError, type InternalAxiosRequestConfig } from 'axios';
import AsyncStorage from '@react-native-async-storage/async-storage';
import { Platform } from 'react-native';
import type { ApiError } from './types';

const TOKEN_KEY = '@emdia:access_token';
const BASE_URL_KEY = '@emdia:base_url';

// Padrão: 10.0.2.2 para emulador Android, localhost para iOS e Web
export const DEFAULT_API_URL = Platform.select({
  android: 'http://10.0.2.2:5196/api/v1',
  ios: 'http://localhost:5196/api/v1',
  default: 'http://localhost:5196/api/v1',
});

let currentAccessToken: string | null = null;
let currentBaseUrl: string = DEFAULT_API_URL;

export const setAccessToken = async (token: string | null) => {
  currentAccessToken = token;
  if (token) {
    await AsyncStorage.setItem(TOKEN_KEY, token);
  } else {
    await AsyncStorage.removeItem(TOKEN_KEY);
  }
};

export const getAccessToken = () => currentAccessToken;

export const setCustomBaseUrl = async (url: string) => {
  currentBaseUrl = url;
  api.defaults.baseURL = url;
  await AsyncStorage.setItem(BASE_URL_KEY, url);
};

export const getBaseUrl = () => currentBaseUrl;

export const api = axios.create({
  baseURL: currentBaseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 15000,
});

// Inicialização síncrona/assíncrona do token guardado
export const loadStoredAuth = async () => {
  try {
    const savedUrl = await AsyncStorage.getItem(BASE_URL_KEY);
    if (savedUrl) {
      currentBaseUrl = savedUrl;
      api.defaults.baseURL = savedUrl;
    }
    const token = await AsyncStorage.getItem(TOKEN_KEY);
    if (token) {
      currentAccessToken = token;
    }
    return token;
  } catch {
    return null;
  }
};

// Interceptor de requisição: injeta Bearer token
api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  if (currentAccessToken && config.headers) {
    config.headers.Authorization = `Bearer ${currentAccessToken}`;
  }
  return config;
});

// Interceptor de resposta para extrair erros amigáveis
api.interceptors.response.use(
  (response) => response,
  (error: AxiosError<ApiError>) => {
    return Promise.reject(error);
  }
);
