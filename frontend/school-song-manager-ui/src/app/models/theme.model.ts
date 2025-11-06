export interface Theme {
  id: string;
  name: string;
  color: string;
}

export interface CreateThemeRequest {
  name: string;
  color: string;
}

export interface UpdateThemeRequest {
  name: string;
  color: string;
}