
import { useState } from 'react';
import { FaBan } from 'react-icons/fa';

const cancellationReasons = [
  'Bad weather',
  'Due to rain',
  'Day light off',
  'Extreme heat',
  'Players unavailable',
  'Ground unavailable',
  'Playing kit unavailable',
  'Others',
];

export default function CancelMatchModal({
  match,
  isSubmitting = false,
  error = '',
  onClose,
  onConfirm,
}) {
  const [step, setStep] = useState('reason');
  const [selectedReason, setSelectedReason] = useState('');
  const [otherExplanation, setOtherExplanation] = useState('');
  const [validationError, setValidationError] = useState('');

  if (!match) return null;

  const getValue = (...keys) => {
    for (const key of keys) {
      if (match[key] != null) return match[key];
    }
    return null;
  };

  const matchName =
    getValue('name', 'Name') ||
    `${getValue('teamAName', 'TeamAName') || 'Team A'} vs ${
      getValue('teamBName', 'TeamBName') || 'Team B'
    }`;

  const reason =
    selectedReason === 'Others'
      ? `Others: ${otherExplanation.trim()}`
      : selectedReason;

  function continueCancellation() {
    if (!selectedReason) {
      setValidationError('Please select a cancellation reason.');
      return;
    }

    if (
      selectedReason === 'Others' &&
      !otherExplanation.trim()
    ) {
      setValidationError('Please explain the cancellation reason.');
      return;
    }

    if (otherExplanation.trim().length > 40) {
      setValidationError('Explanation cannot exceed 40 characters.');
      return;
    }

    setValidationError('');
    setStep('confirm');
  }

  return (
    <div
      className="cancel-modal-backdrop"
      onMouseDown={(event) => {
        if (
          event.target === event.currentTarget &&
          !isSubmitting
        ) {
          onClose();
        }
      }}
    >
      <section
        className="cancel-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="cancel-modal-title"
      >
        <div className="cancel-modal-heading">
          <span className="cancel-modal-icon">
            <FaBan />
          </span>

          <div>
            <h2 id="cancel-modal-title">
              {step === 'reason'
                ? 'Cancel match'
                : 'Confirm cancellation'}
            </h2>

            <p>
              {step === 'reason'
                ? 'Select the reason for cancelling this match.'
                : 'This action will mark the match as cancelled.'}
            </p>
          </div>
        </div>

        {step === 'reason' ? (
          <>
            <div className="cancel-reason-list">
              {cancellationReasons.map((item) => (
                <label
                  className={`cancel-reason-option ${
                    selectedReason === item ? 'selected' : ''
                  }`}
                  key={item}
                >
                  <input
                    type="radio"
                    name="cancellationReason"
                    value={item}
                    checked={selectedReason === item}
                    onChange={() => {
                      setSelectedReason(item);
                      setValidationError('');
                    }}
                  />
                  <span>{item}</span>
                </label>
              ))}
            </div>

            {selectedReason === 'Others' && (
              <div className="cancel-explanation">
                <label htmlFor="cancel-explanation">
                  Explain the reason *
                </label>

                <textarea
                  id="cancel-explanation"
                  rows={3}
                  maxLength={40}
                  value={otherExplanation}
                  onChange={(event) => {
                    setOtherExplanation(event.target.value);
                    setValidationError('');
                  }}
                  placeholder="Enter the cancellation reason..."
                />

                <small>
                  {otherExplanation.length}/40 characters
                </small>
              </div>
            )}
          </>
        ) : (
          <>
            <div className="cancel-confirm-details">
              <span className="cancel-detail-label">Match</span>
              <strong>{matchName}</strong>

              <span className="cancel-detail-label">Reason</span>
              <p>{reason}</p>
            </div>

            <p className="cancel-confirm-warning">
              Are you sure you want to cancel this match?
            </p>
          </>
        )}

        {(validationError || error) && (
          <p className="cancel-modal-error" role="alert">
            {validationError || error}
          </p>
        )}

        <div className="cancel-modal-actions">
          <button
            type="button"
            className="cancel-modal-secondary"
            disabled={isSubmitting}
            onClick={() => {
              if (step === 'confirm') {
                setStep('reason');
                setValidationError('');
              } else {
                onClose();
              }
            }}
          >
            {step === 'confirm' ? 'Back' : 'Keep match'}
          </button>

          {step === 'reason' ? (
            <button
              type="button"
              className="cancel-modal-danger"
              onClick={continueCancellation}
            >
              Continue
            </button>
          ) : (
            
          <button
            type="button"
            className="cancel-modal-danger"
            disabled={isSubmitting}
            onClick={() =>
              onConfirm({
                cancellationReasonType: 'Manual',
                cancellationReason: reason,
              })
            }
          >
            {isSubmitting ? 'Cancelling…' : 'Confirm cancellation'}
          </button>

          )}
        </div>
      </section>
    </div>
  );
}
