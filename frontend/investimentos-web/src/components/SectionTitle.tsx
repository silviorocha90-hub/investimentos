interface SectionTitleProps {
  title: string
  subtitle?: string
  badge?: string
}

export function SectionTitle({
  title,
  subtitle,
  badge,
}: SectionTitleProps) {
  return (
    <div className="section-title">
      <div>
        <h2>{title}</h2>

        {subtitle ? (
          <p>{subtitle}</p>
        ) : null}
      </div>

      {badge ? (
        <span className="badge">
          {badge}
        </span>
      ) : null}
    </div>
  )
}