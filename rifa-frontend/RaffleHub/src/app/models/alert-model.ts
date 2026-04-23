export type NotificationType = 'success' | 'error';
export type NotificationMode = 'toast' | 'modal';

export interface AlertModel {
  type: NotificationType;
  message: string;
  mode: NotificationMode;
  duration?: number
}

