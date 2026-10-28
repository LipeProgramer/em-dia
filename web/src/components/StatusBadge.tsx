import { StatusItem } from '../api/types.ts';
import { Icon, type IconName } from './Icon.tsx';

interface StatusBadgeProps {
  status: StatusItem;
  className?: string;
}

export function StatusBadge({ status, className = '' }: StatusBadgeProps) {
  let label = 'Em dia';
  let icon: IconName = 'check';
  let badgeClass = 'badge-em-dia';
  let ariaLabel = 'Status: Em dia';

  switch (status) {
    case StatusItem.Vencido:
      label = 'Vencido';
      icon = 'close';
      badgeClass = 'badge-vencido';
      ariaLabel = 'Status: Vencido';
      break;
    case StatusItem.Atencao:
      label = 'Atenção';
      icon = 'alert-triangle';
      badgeClass = 'badge-atencao';
      ariaLabel = 'Status: Atenção';
      break;
    case StatusItem.EmDia:
      label = 'Em dia';
      icon = 'check';
      badgeClass = 'badge-em-dia';
      ariaLabel = 'Status: Em dia';
      break;
    case StatusItem.Encerrado:
      label = 'Encerrado';
      icon = 'minus';
      badgeClass = 'badge-encerrado';
      ariaLabel = 'Status: Encerrado';
      break;
  }

  return (
    <span
      className={`status-badge ${badgeClass} ${className}`}
      role="status"
      aria-label={ariaLabel}
    >
      <span className="status-badge-icon" aria-hidden="true">
        <Icon name={icon} size={13} strokeWidth={2.6} />
      </span>
      <span className="status-badge-text">{label}</span>
    </span>
  );
}
