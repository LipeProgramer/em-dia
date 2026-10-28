import type { ReactNode } from 'react';

export type IconName =
  | 'radar'
  | 'list'
  | 'car'
  | 'bike'
  | 'settings'
  | 'logout'
  | 'plus'
  | 'arrow-right'
  | 'arrow-left'
  | 'history'
  | 'check'
  | 'minus'
  | 'check-circle'
  | 'calendar'
  | 'alert-triangle'
  | 'alert-circle'
  | 'focus'
  | 'sparkles'
  | 'user'
  | 'bell'
  | 'paw'
  | 'monitor'
  | 'droplet'
  | 'id-card'
  | 'folder'
  | 'clock'
  | 'chevron-down'
  | 'chevron-right'
  | 'close'
  | 'trash'
  | 'edit'
  | 'gauge'
  | 'shield-check'
  | 'lightbulb'
  | 'mail'
  | 'lock'
  | 'eye'
  | 'eye-off';

interface IconProps {
  name: IconName;
  size?: number;
  strokeWidth?: number;
  className?: string;
}

const paths: Record<IconName, ReactNode> = {
  radar: <><circle cx="12" cy="12" r="9" /><path d="M12 3v9l6.4 6.4" /><path d="M12 7a5 5 0 1 0 5 5" /><circle cx="12" cy="12" r="1" /></>,
  list: <><path d="M9 6h11M9 12h11M9 18h11" /><path d="M4 6h.01M4 12h.01M4 18h.01" /></>,
  car: <><path d="m5 11 1.5-4.5A2 2 0 0 1 8.4 5h7.2a2 2 0 0 1 1.9 1.5L19 11" /><path d="M3 11h18v7H3z" /><path d="M6 18v2m12-2v2M6 14h.01M18 14h.01" /></>,
  bike: <><circle cx="5.5" cy="17.5" r="3.5" /><circle cx="18.5" cy="17.5" r="3.5" /><path d="m9 17.5 3-7 3 7H9l-2-5h4m1-3h3" /></>,
  settings: <><circle cx="12" cy="12" r="3" /><path d="m19.4 15 .1.1 1.4 1.1-1.4 2.4-1.7-.6a8 8 0 0 1-1.6.9l-.3 1.8h-2.8l-.3-1.8a8 8 0 0 1-1.6-.9l-1.7.6-1.4-2.4 1.4-1.1a7 7 0 0 1 0-1.9l-1.4-1.1 1.4-2.4 1.7.6a8 8 0 0 1 1.6-.9l.3-1.8h2.8l.3 1.8a8 8 0 0 1 1.6.9l1.7-.6 1.4 2.4-1.4 1.1a7 7 0 0 1 0 1.8Z" transform="translate(-1 -1) scale(1.08)" /></>,
  logout: <><path d="M10 17l5-5-5-5M15 12H3" /><path d="M12 3h6a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2h-6" /></>,
  plus: <><path d="M12 5v14M5 12h14" /></>,
  'arrow-right': <><path d="M5 12h14M13 6l6 6-6 6" /></>,
  'arrow-left': <><path d="M19 12H5M11 18l-6-6 6-6" /></>,
  history: <><path d="M3 12a9 9 0 1 0 2.6-6.4L3 8" /><path d="M3 3v5h5M12 7v5l3 2" /></>,
  check: <path d="m5 12 4 4L19 6" />,
  minus: <path d="M5 12h14" />,
  'check-circle': <><circle cx="12" cy="12" r="9" /><path d="m8 12 2.5 2.5L16 9" /></>,
  calendar: <><rect x="3" y="5" width="18" height="16" rx="2" /><path d="M16 3v4M8 3v4M3 10h18" /></>,
  'alert-triangle': <><path d="m10.3 4.3-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.7-2.7l-8-14a2 2 0 0 0-3.4 0Z" /><path d="M12 9v4m0 4h.01" /></>,
  'alert-circle': <><circle cx="12" cy="12" r="9" /><path d="M12 8v4m0 4h.01" /></>,
  focus: <><circle cx="12" cy="12" r="9" /><circle cx="12" cy="12" r="5" /><circle cx="12" cy="12" r="1" /></>,
  sparkles: <><path d="m12 3 1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8L12 3Z" /><path d="m19 15 .9 2.1L22 18l-2.1.9L19 21l-.9-2.1L16 18l2.1-.9L19 15ZM5 2l.7 1.3L7 4l-1.3.7L5 6l-.7-1.3L3 4l1.3-.7L5 2Z" /></>,
  user: <><circle cx="12" cy="8" r="3.5" /><path d="M5 20a7 7 0 0 1 14 0" /></>,
  bell: <><path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9" /><path d="M10 21h4" /></>,
  paw: <><circle cx="7.5" cy="8" r="1.8" /><circle cx="16.5" cy="8" r="1.8" /><circle cx="5" cy="13" r="1.7" /><circle cx="19" cy="13" r="1.7" /><path d="M8 18c0-2.2 1.8-4.2 4-4.2s4 2 4 4.2-1.5 3-4 3-4-0.8-4-3Z" /></>,
  monitor: <><rect x="3" y="4" width="18" height="13" rx="2" /><path d="M8 21h8m-4-4v4" /></>,
  droplet: <><path d="M12 3s7 7.2 7 12a7 7 0 0 1-14 0c0-4.8 7-12 7-12Z" /><path d="M9 16a3 3 0 0 0 3 3" /></>,
  'id-card': <><rect x="3" y="5" width="18" height="14" rx="2" /><circle cx="8" cy="11" r="2" /><path d="M5.5 16a3 3 0 0 1 5 0M13 10h5m-5 4h5" /></>,
  folder: <><path d="M3 7a2 2 0 0 1 2-2h5l2 2h7a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2Z" /><path d="M3 10h18" /></>,
  clock: <><circle cx="12" cy="12" r="9" /><path d="M12 7v5l3 2" /></>,
  'chevron-down': <path d="m6 9 6 6 6-6" />,
  'chevron-right': <path d="m9 18 6-6-6-6" />,
  close: <><path d="m6 6 12 12M18 6 6 18" /></>,
  trash: <><path d="M3 6h18M8 6V4h8v2m3 0-1 14H6L5 6m4 4v6m6-6v6" /></>,
  edit: <><path d="m14 5 5 5M4 20l4.5-1 11-11a2.1 2.1 0 0 0-3-3l-11 11L4 20Z" /></>,
  gauge: <><path d="M4.5 19a9 9 0 1 1 15 0" /><path d="m12 13 4-4M7 19h10" /></>,
  'shield-check': <><path d="M12 22s8-4 8-11V5l-8-3-8 3v6c0 7 8 11 8 11Z" /><path d="m9 12 2 2 4-4" /></>,
  lightbulb: <><path d="M9 18h6m-5 4h4M8 14.5A6 6 0 1 1 16 14c-.8.6-1 1.4-1 2H9c0-.7-.3-1.1-1-1.5Z" /></>,
  mail: <><rect x="3" y="5" width="18" height="14" rx="2" /><path d="m3 7 9 6 9-6" /></>,
  lock: <><rect x="4" y="10" width="16" height="11" rx="2" /><path d="M8 10V7a4 4 0 0 1 8 0v3m-4 5v2" /></>,
  eye: <><path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6-10-6-10-6Z" /><circle cx="12" cy="12" r="2.5" /></>,
  'eye-off': <><path d="m3 3 18 18M10.6 10.6a2 2 0 0 0 2.8 2.8" /><path d="M9.9 5.2A10.8 10.8 0 0 1 12 5c6.5 0 10 7 10 7a15.4 15.4 0 0 1-3.1 3.7M6.2 6.2C3.4 8.1 2 12 2 12s3.5 7 10 7a10 10 0 0 0 4.1-.9" /></>,
};

export function Icon({ name, size = 20, strokeWidth = 1.8, className }: IconProps) {
  return (
    <svg
      aria-hidden="true"
      className={className}
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth={strokeWidth}
      strokeLinecap="round"
      strokeLinejoin="round"
      focusable="false"
    >
      {paths[name]}
    </svg>
  );
}
