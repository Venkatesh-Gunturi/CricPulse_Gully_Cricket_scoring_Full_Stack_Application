import { useEffect } from 'react';
import { FaCheckCircle, FaTimes } from 'react-icons/fa';
import '../styles/toast.css';

export default function Toast({ message, type = 'success', onClose, duration = 3000 }) {
  useEffect(() => {
    if (!message) return undefined;
    const timer = window.setTimeout(() => onClose?.(), duration);
    return () => window.clearTimeout(timer);
  }, [message, duration, onClose]);

  if (!message) return null;
  return (
    <div className={`app-toast app-toast-${type}`} role="status" aria-live="polite">
      <FaCheckCircle aria-hidden="true" />
      <span>{message}</span>
      <button type="button" aria-label="Dismiss notification" onClick={onClose}><FaTimes /></button>
    </div>
  );
}
